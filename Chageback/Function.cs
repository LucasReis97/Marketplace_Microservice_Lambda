using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Newtonsoft.Json;
using Shared.Enums;
using Shared.Models;
using Shared.Utils;
using System.Threading.Tasks;
using static Amazon.Lambda.SNSEvents.SNSEvent;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Chageback
{
    public class Function
    {
        public Function()
        {
        }

        public async Task FunctionHandler(SNSEvent evnt, ILambdaContext context)
        {
            foreach (var record in evnt.Records)
            {
                await ProcessRecordAsync(record, context);
            }
        }

        private async Task ProcessRecordAsync(SNSRecord record, ILambdaContext context)
        {
            //Need implement api to refund
            var order = JsonConvert.DeserializeObject<Order>(record.Sns.Message);
            order.OrderStatus = EnumOrderStatus.Chargeback;
            await order.SaveOrder();
            context.Logger.LogLine($"Successfully refunded the money to the customer: {record.Sns.Message}");
        }
    }
}
