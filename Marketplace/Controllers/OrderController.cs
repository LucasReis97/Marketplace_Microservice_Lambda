using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Utils;
using System;
using System.Threading.Tasks;

namespace Marketplace.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        [HttpPost]
        public async Task Post([FromBody] Order order)
        {
            order.Id = Guid.NewGuid().ToString();
            order.CreatedDate = DateTime.UtcNow;

            await order.SaveOrder();

            Console.WriteLine($"Order created successfully: id {order.Id}");
        }
    }
}
