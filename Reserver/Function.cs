using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using Shared.Enums;
using Shared.Models;
using Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Amazon.Lambda.SQSEvents.SQSEvent;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Reserver
{
    public class Function
    {
        private AmazonDynamoDBClient AmazonDynamoDBClient { get; }
        public Function()
        {
            AmazonDynamoDBClient = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
        }

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            if (evnt.Records.Count > 1) throw new InvalidOperationException("Only one message at a time can be processed");
            var message = evnt.Records.FirstOrDefault();
            if (message == null) return;
            await ProcessMessageAsync(message, context);
        }

        private async Task ProcessMessageAsync(SQSMessage message, ILambdaContext context)
        {
            var order = JsonConvert.DeserializeObject<Order>(message.Body);
            order.OrderStatus = EnumOrderStatus.Reserved;
            foreach (var product in order.Products)
            {
                try
                {
                    await ReduceStockProducts(product);
                    product.Reserved = true;
                    context.Logger.LogLine($"Product reduced from stock successfully - id:{product.Id}, quantity:{product.Quantity}");
                }
                catch
                {
                    order.Canceled = true;
                    order.CanceledReason = $"Product out of stock - id:{product.Id}, quantity:{product.Quantity}";
                    context.Logger.LogLine("Error: " + order.CanceledReason);
                    await OrderCanceled(order, context);
                    break;
                }
            }
            if (!order.Canceled) await AmazonUtils.SendToQueue(EnumQueueSQS.reserved, order);
            await order.SaveOrder();
        }

        private async Task OrderCanceled(Order order, ILambdaContext context)
        {
            foreach (var product in order.Products.Where(x => x.Reserved))
            {
                await ReturnToStockProducts(product);
                product.Reserved = false;
                context.Logger.LogLine($"Product returned to stock successfully - id:{product.Id}, quantity:{product.Quantity}");
            }
            await AmazonUtils.SendToQueue(EnumQueueSNS.failure, order);
        }

        private async Task ReduceStockProducts(Product product)
        {
            var request = new UpdateItemRequest
            {
                TableName = "products",
                ReturnValues = "NONE",
                Key = new Dictionary<string, AttributeValue>
                {
                    {"Id",new AttributeValue{S= product.Id}}
                },
                UpdateExpression = "SET Quantity = (Quantity - :quantityWanted)",
                ConditionExpression = "Quantity >= :quantityWanted",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":quantityWanted", new AttributeValue { N = product.Quantity.ToString() }}
                }
            };
            await AmazonDynamoDBClient.UpdateItemAsync(request);
        }

        private async Task ReturnToStockProducts(Product product)
        {
            var request = new UpdateItemRequest
            {
                TableName = "products",
                ReturnValues = "NONE",
                Key = new Dictionary<string, AttributeValue>
                {
                    {"Id",new AttributeValue{S= product.Id}}
                },
                UpdateExpression = "SET Quantity = (Quantity + :quantityToReturn)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":quantityToReturn", new AttributeValue { N = product.Quantity.ToString() }}
                }
            };
            await AmazonDynamoDBClient.UpdateItemAsync(request);
        }
    }
}
