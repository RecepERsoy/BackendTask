namespace Product.Application.Events
{
    /// <summary>
    /// Ürün eklendiğinde sistem genelinde yayınlanacak asenkron olay mesajı.
    /// Bu sınıf, diğer mikroservislerin (örn: Log Servisi) ihtiyaç duyacağı temel verileri taşır.
    /// </summary>
    public class ProductAddedEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}