using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OrderManagement.Controllers;
using OrderManagement.Models;
using OrderManagement.Services;
using System;
using System.Linq;
using System.Net;

namespace OrderManagement.Tests.Controllers
{
    internal static class OrderControllerTests
    {
        private static IContext _context =
            Substitute.For<IContext>();
        private readonly static ILogger<OrdersController> _logger =
            Substitute.For<ILogger<OrdersController>>();
        private static IDatabase _database =
            Substitute.For<IDatabase>();
        private static OrdersController _controller = new(_logger, _database);

        private static void SetUp()
        {
            _database = Substitute.For<IDatabase>();
            _context = Substitute.For<IContext>();
            _controller = new OrdersController(_logger, _database);
        }

        internal class CreateOrder
        {
            [SetUp]
            public void SetUp()
            {
                OrderControllerTests.SetUp();
            }

            [Test]
            public void ShouldCreateNewOrderWithSuccess()
            {
                // arrange
                var newOrder = new OrdersController.CreateOrderDto();
                _database.GetContext().Returns(_context);

                // act
                var result = _controller.CreateOrder(newOrder);

                // assert
                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.Not.Null);
                    _context.Orders.Received().Add(result,
                        Arg.Is<Order>(order => order.Id == result));
                });
            }
        }

        internal class DeleteOrder
        {
            [SetUp]
            public void SetUp()
            {
                OrderControllerTests.SetUp();
            }

            [Test]
            public void ShouldDeleteAnExistentOrderWithSuccess()
            {
                // arrange
                var order = new Order { Id = Guid.NewGuid() };
                _database.GetContext().Returns(_context);
                _context.Orders.TryGetValue(order.Id, out _).Returns(x =>
                {
                    x[1] = order;
                    return true;
                });

                // act
                var result = (dynamic)_controller.DeleteOrder(order.Id);

                // assert
                Assert.That(result.StatusCode,
                    Is.EqualTo(HttpStatusCode.OK.GetHashCode()));
            }

            [Test]
            public void ShouldNotDeleteAnInexistentOrderAndReturnNotFoundResult()
            {
                // arrange
                var orderId = Guid.NewGuid();
                _database.GetContext().Returns(_context);
                _context.Orders.TryGetValue(orderId, out _).Returns(false);

                // act
                var result = (dynamic)_controller.DeleteOrder(orderId);

                // assert
                Assert.That(result.StatusCode,
                    Is.EqualTo(HttpStatusCode.NotFound.GetHashCode()));
            }
        }

        internal class GetOrder
        {
            [SetUp]
            public void SetUp()
            {
                OrderControllerTests.SetUp();
            }

            [Test]
            public void ShouldGetOrderByIdWithSuccess()
            {
                // arrange
                var order = new Order { Id = Guid.NewGuid() };
                _database.GetContext(true).Returns(_context);
                _context.Orders.TryGetValue(order.Id, out _).Returns(x =>
                {
                    x[1] = order;
                    return true;
                });

                // act
                var result = _controller.GetOrder(order.Id);

                // assert
                Assert.That(result.Value, Is.EqualTo(order));
            }

            [Test]
            public void ShouldNotGetInexistentOrderAndReturnNotFound()
            {
                // arrange
                var orderId = Guid.NewGuid();
                _database.GetContext(true).Returns(_context);
                _context.Orders.TryGetValue(orderId, out _).Returns(false);

                // act
                var result = (dynamic)_controller.GetOrder(orderId);

                // assert
                Assert.That(result.Result.StatusCode,
                    Is.EqualTo(HttpStatusCode.NotFound.GetHashCode()));
            }
        }

        internal class GetOrders
        {
            [SetUp]
            public void SetUp()
            {
                OrderControllerTests.SetUp();
            }

            [Test]
            public void ShouldReturnFirstPageOfAllOrders()
            {
                // arrange
                var order = new Order { Id = Guid.NewGuid() };
                _database.GetContext(true).Returns(_context);
                _context.Orders.Values.Returns(new Order[] { order });

                // act
                var result = _controller.GetOrders();

                // assert
                Assert.That(result.Orders, Has.Exactly(1)
                    .Matches<Order>(o => o.Id == order.Id));
            }

            [Test]
            public void ShouldReturnSecondPageOfAllOrders()
            {
                // arrange
                var orders = Enumerable.Range(1, 11).Select(i => new Order
                {
                    Id = Guid.NewGuid(),
                    Date = DateTimeOffset.Now.AddSeconds(i)
                }).ToArray();
                var paginatedLastOrder = orders.Skip(1).First();
                var secondPageFirstOrder = orders.First();
                var dictionaryOfOrders = orders
                    .ToDictionary(order => order.Id, order => order);
                _database.GetContext(true).Returns(_context);
                _context.Orders.Returns(dictionaryOfOrders);

                // act
                var result = _controller.GetOrders(paginatedLastOrder.Id,
                    paginatedLastOrder.Date);

                // assert
                Assert.That(result.Orders, Has.Exactly(1)
                    .Matches<Order>(o => o.Id == secondPageFirstOrder.Id));
            }
        }

        internal class UpdateDeliveryAddress
        {
            [SetUp]
            public void SetUp()
            {
                OrderControllerTests.SetUp();
            }

            [Test]
            public void ShouldUpdateOrderDeliveryAddressWithSuccess()
            {
                // arrange
                var order = new Order {
                    Id = Guid.NewGuid(),
                    DeliveryAddress = "Old"
                };
                const string newDeliveryAddress = "New";
                _database.GetContext().Returns(_context);
                _context.Orders.TryGetValue(order.Id, out _).Returns(x =>
                {
                    x[1] = order;
                    return true;
                });

                // act
                var result = (dynamic)_controller.UpdateDeliveryAddress(order.Id,
                    newDeliveryAddress);

                // assert
                Assert.Multiple(() =>
                {
                    Assert.That(result.StatusCode,
                        Is.EqualTo(HttpStatusCode.OK.GetHashCode()));
                    Assert.That(order.DeliveryAddress, Is.EqualTo(newDeliveryAddress));
                });
            }

            [Test]
            public void ShouldNotUpdateInexistentOrderDeliveryAddressAndReturnNotFound()
            {
                // arrange
                var orderId = Guid.NewGuid();
                const string newDeliveryAddress = "New";
                _database.GetContext().Returns(_context);
                _context.Orders.TryGetValue(orderId, out _).Returns(false);

                // act
                var result = (dynamic)_controller.UpdateDeliveryAddress(orderId,
                    newDeliveryAddress);

                // assert
                Assert.That(result.StatusCode,
                    Is.EqualTo(HttpStatusCode.NotFound.GetHashCode()));
            }
        }

        internal class UpdateOrderItems
        {
            [SetUp]
            public void SetUp()
            {
                OrderControllerTests.SetUp();
            }

            [Test]
            public void ShouldUpdateOrderItemsAddressWithSuccess()
            {
                // arrange
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    Items = Array.Empty<Guid>()
                };
                var newItems = new Guid[] { new() };
                _database.GetContext().Returns(_context);
                _context.Orders.TryGetValue(order.Id, out _).Returns(x =>
                {
                    x[1] = order;
                    return true;
                });

                // act
                var result = (dynamic)_controller.UpdateOrderItems(order.Id, newItems);

                // assert
                Assert.Multiple(() =>
                {
                    Assert.That(result.StatusCode,
                        Is.EqualTo(HttpStatusCode.OK.GetHashCode()));
                    Assert.That(order.Items, Is.EqualTo(newItems));
                });
            }

            [Test]
            public void ShouldNotUpdateInexistentOrderItemsAddressAndReturnNotFound()
            {
                // arrange
                var orderId = Guid.NewGuid();
                var newItems = new Guid[] { new() };
                _database.GetContext().Returns(_context);
                _context.Orders.TryGetValue(orderId, out _).Returns(false);

                // act
                var result = (dynamic)_controller.UpdateOrderItems(orderId, newItems);

                // assert
                Assert.That(result.StatusCode,
                    Is.EqualTo(HttpStatusCode.NotFound.GetHashCode()));
            }
        }
    }
}
