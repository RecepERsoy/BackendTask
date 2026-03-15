using System.Threading.Tasks;

namespace Product.Application.Interfaces.Repositories
{
 
    public interface IProductRepository
    {
        Task<Domain.Entities.Product> AddAsync(Domain.Entities.Product product);
    }
}
