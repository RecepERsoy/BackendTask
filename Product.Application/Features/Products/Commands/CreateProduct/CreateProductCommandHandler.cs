using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Events;
using Product.Application.Interfaces.Repositories;

namespace Product.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Yeni ürün oluşturma sürecini yöneten Handler. 
    /// Veritabanı kaydı sonrası Cache Invalidation (Redis) ve Asenkron Mesajlaşma (RabbitMQ) adımlarını yürütür.
    /// </summary>
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IDistributedCache _cache;

        // Düzenleme: IPublishEndpoint artık parametre olarak içeri alınıyor (Dependency Injection)
        public CreateProductCommandHandler(IProductRepository productRepository, IDistributedCache cache, IPublishEndpoint publishEndpoint)
        {
            _productRepository = productRepository;
            _cache = cache;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Yeni Domain nesnesinin oluşturulması
            var newProduct = new Domain.Entities.Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Stock = request.Stock,
                CreatedDate = DateTime.UtcNow
            };

            // 2. Persistence: SQL Veritabanına yazma işlemi
            await _productRepository.AddAsync(newProduct);

            // 3. Cache Invalidation: Liste güncelliği için Redis'teki anahtarı temizle
            await _cache.RemoveAsync("productList", cancellationToken);

            // 4. Event Publishing: RabbitMQ üzerinden diğer mikroservisleri asenkron bilgilendir
            // Not: Return'den ÖNCE yapılmalıdır.
            await _publishEndpoint.Publish(new ProductAddedEvent
            {
                Id = newProduct.Id,
                Name = newProduct.Name,
                Price = newProduct.Price,
                CreatedDate = newProduct.CreatedDate
            }, cancellationToken);

            // 5. İşlem sonucunda üretilen kimliği geri dön
            return newProduct.Id;
        }
    }
}