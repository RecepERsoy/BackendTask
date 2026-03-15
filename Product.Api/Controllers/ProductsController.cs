using MediatR;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Features.Products.Commands.CreateProduct;
using System.Threading.Tasks;

namespace Product.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

      
        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

      
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {

            var productId = await _mediator.Send(command);

            return Ok(new { Id = productId, Message = "Ürün başarıyla eklendi." });
        }
    }
}