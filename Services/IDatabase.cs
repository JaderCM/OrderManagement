using static OrderManagement.Services.Database;

namespace OrderManagement.Services
{
    public interface IDatabase
    {
        public Context GetContext(bool readOnly = false);
    }
}