namespace LegacyInventory.Core.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Sku { get; set; }

        public string Name { get; set; }

        public decimal UnitPrice { get; set; }

        public int QuantityOnHand { get; set; }

        public System.DateTime LastUpdated { get; set; }
    }
}
