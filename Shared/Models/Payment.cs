using System;

namespace Shared.Models
{
    public class Payment
    {
        public string CardNumber { get; set; }

        public DateTime ExpiringDate { get; set; }

        public string CVV { get; set; }
    }
}
