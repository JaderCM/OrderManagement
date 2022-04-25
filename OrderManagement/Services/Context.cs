using OrderManagement.Models;

namespace OrderManagement.Services
{
    public partial class Database
    {
        public class Context : IContext, IDisposable
        {
            public IDictionary<Guid, Order> Orders { get; set; } = new Dictionary<Guid, Order>();
            public IDictionary<Guid, Product> Products { get; set; } = new Dictionary<Guid, Product>();

            public void Dispose()
            {
                if (!ReadOnly)
                    lock (_context)
                        SaveData();
            }
        }
    }
}
