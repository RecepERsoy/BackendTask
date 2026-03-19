using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}