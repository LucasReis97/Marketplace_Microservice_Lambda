
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Shared.Enums;
using Shared.Models;
using Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Receiver
{
    public class Function
    {

        public async Task FunctionHandler(DynamoDBEvent dynamoDbEvent, ILambdaContext context)
        {
            foreach (var record in dynamoDbEvent.Records)
            {
                if (record.EventName == "INSERT")
                {
                    var order = record.Dynamodb.NewImage.ToObject<Order>();
                    order.OrderStatus = EnumOrderStatus.Received;
                    try
                    {
                        await UpdateAmountOrder(order);
                        await AmazonUtils.SendToQueue(EnumQueueSQS.order, order);
                        context.Logger.LogLine($"Order collection with success - id: {order.Id}");

                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogLine($"Error: '{ex.Message}'");
                        order.CanceledReason = ex.Message;
                        order.Canceled = true;
                        await AmazonUtils.SendToQueue(EnumQueueSNS.failure, order);
                    }
                    await order.SaveOrder();
                }
            }
        }

        private async Task UpdateAmountOrder(Order order)
        {
            foreach (var product in order.Products)
            {
                var productStock = await GetProductDynamo(product.Id);
                if (productStock == null) throw new InvalidOperationException($"Product not found - id: {product.Id}");
                product.Name = productStock.Name;
                product.Value = productStock.Value;
            }

            var totalAmountUpdated = order.Products.Sum(x => x.Value * x.Quantity);
            if (order.Amount != 0 && order.Amount != totalAmountUpdated) throw new InvalidOperationException($"The expected value is {totalAmountUpdated} and the sent value was {order.Amount}");
            order.Amount = totalAmountUpdated;
        }

        private async Task<Product> GetProductDynamo(string id)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
            var request = new QueryRequest
            {
                TableName = "products",
                KeyConditionExpression = "Id = :v_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":v_id", new AttributeValue { S = id } } }
            };

            var response = await client.QueryAsync(request);
            var item = response.Items.FirstOrDefault();
            if (item == null) return null;
            return item.ToObject<Product>();
        }
    }
}
