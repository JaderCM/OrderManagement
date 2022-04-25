using static OrderManagement.Services.Database;

namespace OrderManagement.Services
{
    public interface IDatabase
    {
        public IContext GetContext(bool readOnly = false);
    }
}