using Auth.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
    {
        private readonly UserManager<User> _userManager;
        public RegisterCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                CreatedDate = DateTime.UtcNow
            };

            // Kullanıcıyı oluştur ve şifresini otomatik hash'le
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Kayıt Başarısız: {errors}");
            }

            // Yeni kayıt olan herkese standart "User" rolünü ata
            await _userManager.AddToRoleAsync(user, "User");

            return user.Id;
        }
    }
}