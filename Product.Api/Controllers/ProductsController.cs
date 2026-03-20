using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Application.Features.Products.Queries.GetProducts;

namespace Product.Api.Controllers
{
    // Onion mimarisi gereği bu katmanda (API) business logic barındırmıyoruz.
    // Tüm uçlar (endpoints) varsayılan olarak JWT doğrulaması gerektiriyor..
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        // Controller'ı iş sınıflarına sıkı sıkıya bağlamamak (loose coupling) ve 
        // SRP'yi (Single Responsibility) korumak için istekleri MediatR ile yönlendiriyoruz.

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // CQRS (Command): Sadece state değiştiren (yazma) işlemleri izole edildi.
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {

            var productId = await _mediator.Send(command);

            return Ok(new { Id = productId, Message = "Ürün başarıyla eklendi." });
        }

        // CQRS (Query): Sadece veri okuma işlemleri.
        // Not: GetProductsQueryHandler içerisinde okuma performansı için Redis Cache kullanılmıştır.
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            // "Ürünleri getir" mesajını MediatR'a fırlat, o gidip Dapper'lı Handler'ı bulup çalıştıracak
            var query = new GetProductsQuery();
            var products = await _mediator.Send(query);

            return Ok(products);
        }

        // Mevcut bir ürünü günceller. Bu işlem JWT doğrulaması ve yetki gerektirir.
        [HttpPut("update")]
        [Authorize(Policy = "ManagerOrAdminPolicy")] // SADECE TOKEN'I OLANLAR GİREBİLİR!
        public async Task<IActionResult> UpdateProduct([FromBody] Product.Application.Features.Products.Commands.UpdateProduct.UpdateProductCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(new { message = "Ürün başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}