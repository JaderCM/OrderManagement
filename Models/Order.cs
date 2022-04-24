namespace OrderManagement.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public bool Canceled { get; set; }
        public string? DeliveryAddress { get; set; }
        public IEnumerable<Guid> Items { get; set; } = Enumerable.Empty<Guid>();
    }
}