using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrderManagement.Helpers;
using OrderManagement.Models;
using OrderManagement.Services;
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> logger;
        private readonly IDatabase database;

        public OrdersController(ILogger<OrdersController> logger, IDatabase database)
        {
            this.logger = logger;
            this.database = database;
        }

        [HttpPost]
        public Guid CreateOrder(CreateOrderDto order)
        {
            var newOrder = new Order
            {
                Id = Guid.NewGuid(),
                Date = DateTimeOffset.Now,
                DeliveryAddress = order.DeliveryAddress,
                Items = order.Items
            };
            using var context = database.GetContext();
            newOrder.AllocateItemsFromInventory(context.Products);
            context.Orders.Add(newOrder.Id, newOrder);
            return newOrder.Id;
        }

        [HttpDelete("{id:Guid}")]
        public ActionResult DeleteOrder(Guid id)
        {
            using var context = database.GetContext();
            if (!context.Orders.TryGetValue(id, out var order))
                return NotFound();
            order.Canceled = true;
            return Ok();
        }

        [HttpGet("{id:Guid}")]
        public ActionResult<Order> GetOrder(Guid id)
        {
            var context = database.GetContext(true);
            return context.Orders.TryGetValue(id, out var order) ? order : NotFound();
        }

        [HttpGet]
        public GetOrdersDto GetOrders(
            [FromQuery] Guid? paginatedLastOrderId = null,
            [FromQuery] DateTimeOffset? paginatedLastOrderDate = null,
            [FromQuery] int pageSize = 10)
        {
            var paginationToken = paginatedLastOrderId == null || paginatedLastOrderDate == null
                ? null
                : $"{paginatedLastOrderDate.Value.ToUnixTimeSeconds()}{paginatedLastOrderId}";

            using var context = database.GetContext(true);
            var orders = context.Orders.Values
                .OrderByDescending(order => $"{order.Date}{order.Id}")
                .Where(order =>
                {
                    var currentToken = $"{order.Date.ToUnixTimeSeconds()}{order.Id}";
                    var compare = currentToken.CompareTo(paginationToken);
                    return paginationToken == null || compare < 0;
                })
                .Take(pageSize + 1)
                .ToList();

            return new GetOrdersDto
            {
                Orders = orders.Take(pageSize),
                HasMorePages = orders.Count > pageSize
            };
        }

        [HttpPut("{id:Guid}/UpdateDeliveryAddress")]
        public ActionResult UpdateDeliveryAddress(Guid id, [BindRequired] string deliveryAddress)
        {
            using var context = database.GetContext();
            if (!context.Orders.TryGetValue(id, out var order))
                return NotFound();
            order.DeliveryAddress = deliveryAddress;
            return Ok();
        }
        
        [HttpPut("{id:Guid}/UpdateOrderItems")]
        public ActionResult UpdateOrderItems(Guid id, [BindRequired] IEnumerable<Guid> items)
        {
            using var context = database.GetContext();
            if (!context.Orders.TryGetValue(id, out var order))
                return NotFound();
            order.ReallocateItemsFromInventory(context.Products, items);
            order.Items = items;
            return Ok();
        }

        public class CreateOrderDto
        {
            public IEnumerable<Guid> Items { get; set; } = new List<Guid>();
            [Required] public string DeliveryAddress { get; set; } = "Take away";
        }

        public class GetOrdersDto
        {
            public IEnumerable<Order> Orders { get; set; } = new List<Order>();
            public bool HasMorePages { get; set; }
        }
    }
}