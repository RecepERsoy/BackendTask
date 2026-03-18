using Auth.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration; // appsettings.json'daki gizli anahtarı okumak için

        public LoginCommandHandler(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // KULLANICI KONTROLÜ: Veritabanında bu e-postaya sahip biri var mı?
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return "Hata: Kullanıcı bulunamadı!";
            }

            // ŞİFRE KONTROLÜ (BCRYPT): Gelen düz şifre, veritabanındaki hash ile eşleşiyor mu?
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return "Hata: Şifre yanlış!";
            }

            // EŞLEŞME BAŞARILI

            // Kartın içine yazılacak bilgiler (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // appsettings.json'daki gizli anahtarı al
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token'ın kurallarını belirleme (1 saat geçerli)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = creds
            };

            // Token'ı üret
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Token'ı şifreli bir metin olarak dışarıya ver
            return tokenHandler.WriteToken(token);
        }
    }
}