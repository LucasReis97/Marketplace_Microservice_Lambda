using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using Shared.Enums;
using Shared.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Payer
{
    public class Function
    {
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            if (evnt.Records.Count > 1) throw new InvalidOperationException("Only one message at a time can be processed");
            var message = evnt.Records.FirstOrDefault();
            if (message == null) return;
            await ProcessMessageAsync(message, context);
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            var order = JsonConvert.DeserializeObject<Shared.Models.Order>(message.Body);
            if (order.OrderStatus == EnumOrderStatus.Reserved)
            {
                order.OrderStatus = EnumOrderStatus.Paid;
                try
                {
                    //need implement api for payment here
                    await AmazonUtils.SendToQueue(EnumQueueSNS.billed, order);
                    context.Logger.LogLine($"Order paid with successfully - id:{order.Id}");
                }
                catch (Exception ex)
                {
                    context.Logger.LogLine("Error: " + ex.Message);
                    await AmazonUtils.SendToQueue(EnumQueueSNS.failure, order);
                }
                await order.SaveOrder();
            }
            else
            {
                context.Logger.LogLine($"this request has already passed through the payer stage - id: {order.Id}");
                await AmazonUtils.SendToQueue(EnumQueueSNS.billed, order);
                context.Logger.LogLine($"Submitting this request to the next step");
            }
        }


    }
}
