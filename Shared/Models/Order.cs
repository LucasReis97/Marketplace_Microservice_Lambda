using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Enums;
using System;
using System.Collections.Generic;

namespace Shared.Models
{
    [DynamoDBTable("orders")]
    public class Order
    {
        public string Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<Product> Products { get; set; }

        public Client Client { get; set; }

        public Payment Payment { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StatusOrder StatusOrder { get; set; }

        public string CanceledReason { get; set; }

        public bool Canceled { get; set; }

    }
}
