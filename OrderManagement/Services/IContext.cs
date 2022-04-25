using OrderManagement.Models;

namespace OrderManagement.Services
{
    public interface IContext : IDisposable
    {
        IDictionary<Guid, Order> Orders { get; set; }
        IDictionary<Guid, Product> Products { get; set; }
    }
}
