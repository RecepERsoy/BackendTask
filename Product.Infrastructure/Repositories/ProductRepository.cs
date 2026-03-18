using Product.Application.Interfaces.Repositories;
using Product.Infrastructure.Context;

namespace Product.Infrastructure.Repositories
{

    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;


        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Product> AddAsync(Domain.Entities.Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }
        public async Task<Domain.Entities.Product?> GetByIdAsync(Guid id)
        {
            // Veritabanından (örneğin _context.Products üzerinden) ID'ye göre ürünü bul
            return await _context.Products.FindAsync(id);
        }

        public async Task UpdateAsync(Domain.Entities.Product product)
        {
            // Ürünü güncelle ve veritabanına kaydet
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}