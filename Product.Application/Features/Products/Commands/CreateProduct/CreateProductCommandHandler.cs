using MediatR;
using Product.Application.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;

        // Dependency Injection: Handler ayağa kalktığında bana bir IProductRepository ver diyoruz.
        public CreateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
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

          
            await _productRepository.AddAsync(newProduct);

            

            // Eklenen ürünün ID'sini geri dön
            return newProduct.Id;
        }
    }
}