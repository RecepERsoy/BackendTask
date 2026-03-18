using Auth.Domain.Entities;

namespace Auth.Application.Interfaces
{

    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateAsync(User user);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    }
}