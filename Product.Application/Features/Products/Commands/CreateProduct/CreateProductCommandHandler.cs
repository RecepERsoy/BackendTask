using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Application.Features.Products.Commands.CreateProduct
{
    // IRequestHandler<Hangi Komut, Ne Döndürecek>
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        // Handle metodu, CreateProductCommand tetiklendiğinde otomatik çalışır.
        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Gelen request (istek) verileriyle yeni bir Domain Entity'si (Product) oluştur.
            var newProduct = new Domain.Entities.Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Stock = request.Stock,
                CreatedDate = DateTime.UtcNow
            };

            // 2. TODO: Burada veritabanına kaydetme (DbContext) işlemi yapılacak.

            // 3. TODO: Görevde istenen "Ürün eklendikten sonra event fırlatılmalı" işlemi burada yapılacak.

            // 4. Eklenen ürünün ID'sini geri dönüyoruz.
            return await Task.FromResult(newProduct.Id);
        }
    }
}