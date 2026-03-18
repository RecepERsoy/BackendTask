using Auth.Application.Features.Auth.Commands.Login;
using Auth.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    /// Süresi dolan Access Token'ları yenilemek için Refresh Token doğrulamasını ve yeni token üretimini yönetir.
    /// </summary>
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public RefreshTokenCommandHandler(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Gelen Refresh Token veritabanında var mı?
            var user = await _authRepository.GetUserByRefreshTokenAsync(request.RefreshToken);

            // 2. Kullanıcı yoksa veya jetonun süresi (7 gün) dolmuşsa işlemi reddet
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Hata: Geçersiz veya süresi dolmuş Refresh Token. Lütfen tekrar giriş yapın.");
            }

            // 3. YENİ ACCESS TOKEN (JWT) ÜRETİMİ
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.CreateToken(tokenDescriptor);
            string newAccessToken = tokenHandler.WriteToken(jwtToken);

            // 4. YENİ REFRESH TOKEN ÜRETİMİ (Güvenlik için eskiyi iptal edip yenisini veriyoruz)
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            string newRefreshToken = Convert.ToBase64String(randomNumber);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _authRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}