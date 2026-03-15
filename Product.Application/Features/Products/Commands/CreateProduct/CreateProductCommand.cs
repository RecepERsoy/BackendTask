using MediatR;
using System;

namespace Product.Application.Features.Products.Commands.CreateProduct
{
    // IRequest<Guid> demek: "Bu komut çalıştığında bana geriye eklenen ürünün ID'sini (Guid) döndür" demektir.
    public class CreateProductCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}