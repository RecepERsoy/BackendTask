using Microsoft.EntityFrameworkCore;
using Product.Domain.Entities;

namespace Product.Infrastructure.Context
{
    public class ProductDbContext : DbContext
    {
        // Program.cs'den bağlantı ayarlarını alabilmek için Constructor (Yapıcı Metot)
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

        // Veritabanındaki 'Products' tablomuzu temsil edecek property
        public DbSet<Domain.Entities.Product> Products { get; set; }
    }
}
