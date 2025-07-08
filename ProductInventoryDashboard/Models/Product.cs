namespace ProductInventoryDashboard.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? SKU { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public int StockLevel { get; set; }
        public string? Supplier { get; set; }
    }
}
