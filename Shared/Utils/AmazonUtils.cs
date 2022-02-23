using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Shared.Enums;
using Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Utils
{
    public static class AmazonUtils
    {
        public static async Task SaveOrder(this Order order)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
            var context = new DynamoDBContext(client);
            await context.SaveAsync(order);
        }

        public static T ToObject<T>(this Dictionary<string, AttributeValue> dictionary)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
            var context = new DynamoDBContext(client);
            var doc = Document.FromAttributeMap(dictionary);
            return context.FromDocument<T>(doc);
        }

        public static async Task SendToQueue(EnumQueueSQS queue, Order order)
        {
            var json = JsonConvert.SerializeObject(order);
            var client = new AmazonSQSClient(RegionEndpoint.SAEast1);
            var request = new SendMessageRequest
            {
                QueueUrl = $"https://sqs.sa-east-1.amazonaws.com/502028380405/{queue}",
                MessageBody = json
            };
            await client.SendMessageAsync(request);
        }

        public static async Task SendToQueue(EnumQueueSNS queue, Order order)
        {
            var json = JsonConvert.SerializeObject(order);
            var client = new AmazonSimpleNotificationServiceClient(RegionEndpoint.SAEast1);
            var request = new PublishRequest
            {
                Message = json,
                TopicArn = $"arn:aws:sns:sa-east-1:502028380405:{queue}"
            };
            await client.PublishAsync(request);
        }

    }
}
