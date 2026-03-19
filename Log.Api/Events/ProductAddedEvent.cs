namespace Product.Application.Events
{
    public class ProductAddedEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}