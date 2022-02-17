using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Shared.Models;
using System.Threading.Tasks;

namespace Shared.Utils
{
    public static class ExtensionMethods
    {
        public static async Task SaveOrder(this Order order)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
            var context = new DynamoDBContext(client);
            await context.SaveAsync(order);
        }

    }
}
