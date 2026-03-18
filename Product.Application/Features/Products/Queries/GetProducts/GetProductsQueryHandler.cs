using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Product.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<Domain.Entities.Product>>
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;

        // Dependency Injection ile hem Ayarları hem de Redis Cache'i içeri al
        public GetProductsQueryHandler(IConfiguration configuration, IDistributedCache cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<IEnumerable<Domain.Entities.Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            
            const string cacheKey = "productList";

            // ÖNCE REDIS'E BAK: Veri cache'te var mı?
            var cachedProducts = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedProducts))
            {
                // Eğer Redis'te varsa, metni (JSON) listeye çevir ve geri dön.
                return JsonSerializer.Deserialize<IEnumerable<Domain.Entities.Product>>(cachedProducts) ?? new List<Domain.Entities.Product>();
            }

            // REDIS'TE YOKSA: Dapper ile T-SQL sorgusunu çalıştırıp veritabanından çek
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            string sqlQuery = "SELECT Id, Name, Price, Stock, CreatedDate FROM Products ORDER BY CreatedDate DESC";

            IEnumerable<Domain.Entities.Product> products;

            using (var connection = new SqlConnection(connectionString))
            {
                products = await connection.QueryAsync<Domain.Entities.Product>(sqlQuery);
            }

            // ÇEKİLEN VERİYİ REDIS'E KAYDET: Bir dahaki sefere veritabanına gitmemek için cache'e yaz
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // 10 dakika boyunca Redis'te kalsın
            };

            string serializedProducts = JsonSerializer.Serialize(products);
            await _cache.SetStringAsync(cacheKey, serializedProducts, cacheOptions, cancellationToken);

            return products;
        }
    }
}