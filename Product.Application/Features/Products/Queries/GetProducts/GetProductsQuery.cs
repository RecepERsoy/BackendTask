using MediatR;

namespace Product.Application.Features.Products.Queries.GetProducts
{

    public class GetProductsQuery : IRequest<IEnumerable<Domain.Entities.Product>>
    {
    }
}