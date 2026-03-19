using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Events;
using Product.Application.Interfaces.Repositories;

namespace Product.Application.Features.Products.Commands.UpdateProduct
{
    /// <summary>
    /// CQRS Pattern - Ürün güncelleme işlemlerini ve veri tutarlılığını (Data Consistency) yöneten Command Handler.
    /// İşlem başarılı olduğunda Cache Invalidation stratejisini uygulayarak Redis üzerindeki bayat (stale) verileri temizler.
    /// </summary>
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IDistributedCache _cache;

        public UpdateProductCommandHandler(IProductRepository productRepository, IPublishEndpoint publishEndpoint, IDistributedCache cache)
        {
            _productRepository = productRepository;
            _publishEndpoint = publishEndpoint;
            _cache = cache;
        }

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Veri Doğrulama: İşlem yapılacak domain nesnesinin (Entity) kontrolü.
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                throw new Exception("Hata: Güncellenecek ürün bulunamadı!");
            }

            // 2. Durum (State) Güncellemesi
            product.Name = request.Name;
            product.Price = request.Price;
            product.Stock = request.Stock;

            // 3. Kalıcılık (Persistence)
            await _productRepository.UpdateAsync(product);

            // 4. Cache Invalidation (Önbellek İptali)
            // Ürün verisi değiştiği için okuma (Query) işlemlerini besleyen Redis önbelleği temizlenir.
            // Bu sayede bir sonraki listeleme isteğinde veritabanından güncel veriler çekilerek cache yeniden oluşturulur.
            await _cache.RemoveAsync("productList", cancellationToken);
            await _publishEndpoint.Publish(new ProductUpdatedEvent
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                UpdatedDate = DateTime.UtcNow
            }, cancellationToken);

            return true;
        }
    }
}