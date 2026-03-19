namespace Product.Application.Events
{
    /// <summary>
    /// Ürün güncellendiğinde RabbitMQ üzerinden diğer mikroservislere fırlatılacak mesaj taslağı.
    /// </summary>
    public class ProductUpdatedEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}