using OrderManagement.Models;
using System.Text.Json;

namespace OrderManagement.Services
{
    public class Database : IDatabase
    {
        private static readonly Context _context = new();
        private static readonly string _databaseFileName = "database.json";
        private static bool ReadOnly { get; set; }

        private static Context LoadData()
        {
            if (File.Exists(_databaseFileName))
            {
                var json = File.ReadAllText(_databaseFileName);
                var localContext = JsonSerializer.Deserialize<Context>(json);
                _context.Orders = localContext?.Orders ?? new Dictionary<Guid, Order>();
                _context.Products = localContext?.Products ?? new Dictionary<Guid, Product>();
            }

            return _context;
        }

        internal static void SaveData()
        {
            var json = JsonSerializer.Serialize(_context);
            File.WriteAllText(_databaseFileName, json);
        }

        public Context GetContext(bool readOnly = false)
        {
            /// ToDo: Replace lock to critical section avoiding loosing data between LoadData and SaveData.
            lock (_context)
            {
                ReadOnly = readOnly;
                return LoadData();
            }
        }

        public class Context : IDisposable
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
