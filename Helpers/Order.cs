using OrderManagement.Models;

namespace OrderManagement.Helpers
{
    public static class OrderHelper
    {
        public static void AllocateItemsFromInventory(this Order order, IDictionary<Guid, Product> inventory)
        {
            AddProductStock(inventory, order.Items, -1);
        }

        public static void ReallocateItemsFromInventory(this Order order, IDictionary<Guid, Product> inventory, IEnumerable<Guid> newItems)
        {
            // Putting back old order items to inventory
            AddProductStock(inventory, order.Items, 1);
            // Allocating new items from inventory
            AddProductStock(inventory, newItems, -1);
        }

        private static void AddProductStock(IDictionary<Guid, Product> inventory, IEnumerable<Guid> items, int qty)
        {
            foreach(var productId in items)
            {
                var product = GetOrAddInventoryProduct(productId, inventory);
                // Stock should never be negative
                product.StockQty = (uint)Math.Max(0, product.StockQty + qty);
            }
        }

        private static Product GetOrAddInventoryProduct(Guid productId, IDictionary<Guid, Product> inventory)
        {
            if (!inventory.TryGetValue(productId, out var product))
            {
                product = new Product
                {
                    Id = productId,
                    StockQty = 0
                };
                inventory.Add(productId, product);
            }
            return product;
        }
    }
}
