using Auth.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Kimlik doğrulama işlemi sonucunda istemciye iletilecek olan erişim (Access) ve yenileme (Refresh) jetonlarını taşıyan veri transfer nesnesi (DTO).
    /// </summary>
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// CQRS Pattern - Kullanıcı giriş işlemlerini ve token üretim süreçlerini (JWT & Refresh Token) yöneten Command Handler.
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public LoginCommandHandler(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Veri tutarlılığı ve güvenlik kontrolü: E-posta adresi sistemde kayıtlı değilse işlem reddedilir.
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("Hata: Kullanıcı bulunamadı!");
            }

            // Şifre doğrulama: Güvenlik zafiyetlerini önlemek amacıyla düz metin yerine BCrypt algoritması ile hash karşılaştırması yapılır.
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new Exception("Hata: Şifre yanlış!");
            }

            // Erişim Jetonu (Access Token) Üretimi: Kullanıcıya mikroservisler arası yetkilendirme sağlayacak süreli (1 saat) JWT oluşturulur.
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
            string accessToken = tokenHandler.WriteToken(jwtToken);

            // Yenileme Jetonu (Refresh Token) Üretimi ve Kaydı: Access token süresi dolduğunda oturumun kesintisiz devam etmesi için kullanıcıya 7 günlük yeni bir jeton atanır.
            string refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            // Yenileme jetonunun doğrulanabilmesi için güncel bilgilerin veritabanına kalıcı olarak işlenmesi.
            await _authRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Kriptografik olarak güvenli (Cryptographically Secure), 32 byte uzunluğunda rastgele bir yenileme jetonu (Refresh Token) üretir.
        /// </summary>
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}