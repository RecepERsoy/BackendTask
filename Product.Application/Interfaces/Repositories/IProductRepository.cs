namespace Product.Application.Interfaces.Repositories
{

    public interface IProductRepository
    {
        Task<Domain.Entities.Product> AddAsync(Domain.Entities.Product product);
        Task<Domain.Entities.Product?> GetByIdAsync(Guid id); 
        Task UpdateAsync(Domain.Entities.Product product);
    }
}
