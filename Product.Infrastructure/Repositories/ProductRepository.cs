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
    }
}