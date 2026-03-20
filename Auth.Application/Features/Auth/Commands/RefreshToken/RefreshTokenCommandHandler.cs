using Auth.Application.Features.Auth.Commands.Login;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        public RefreshTokenCommandHandler(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Refresh Token'a göre kullanıcıyı bul (UserManager'ın Users listesi üzerinden)
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Geçersiz veya süresi dolmuş Refresh Token.");
            }

            // Microsoft Identity'den kullanıcının rollerini çek
            var userRoles = await _userManager.GetRolesAsync(user);

            // Token içine konulacak bilgileri (Claims) hazırla
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // JWT Token Üretimi
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Refresh Token Üretimi
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var newRefreshToken = Convert.ToBase64String(randomNumber);

            // Kullanıcının yeni Refresh Token'ını veritabanına kaydet
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            // Sonucu dön
            return new AuthResponseDto
            {
                Token = jwtToken,
                RefreshToken = newRefreshToken,
                Expiration = token.ValidTo
            };
        }
    }
}