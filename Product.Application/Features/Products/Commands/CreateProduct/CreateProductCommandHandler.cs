using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Interfaces.Repositories;

namespace Product.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IDistributedCache _cache;

        // Dependency Injection: Handler ayağa kalktığında IProductRepository ve Redis (IDistributedCache) ver.
        public CreateProductCommandHandler(IProductRepository productRepository, IDistributedCache cache)
        {
            _productRepository = productRepository;
            _cache = cache;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Gelen request verileriyle yeni ürün nesnesini oluştur
            var newProduct = new Domain.Entities.Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Stock = request.Stock,
                CreatedDate = DateTime.UtcNow
            };

            // SQL Veritabanına Yaz
            await _productRepository.AddAsync(newProduct);

            //  CACHE INVALIDATION (Redis'teki eski listeyi sil) CACHE INVALIDATION: Veri tutarlılığını sağlamak için Redis'teki eski listeyi sil.

            await _cache.RemoveAsync("productList", cancellationToken);

            // Eklenen ürünün ID'sini geri dön
            return newProduct.Id;
        }
    }
}