using MediatR;

namespace Auth.Application.Features.Auth.Commands.Register
{
    // Bu istek çalıştığında geriye string (mesaj) dönecek.
    public class RegisterCommand : IRequest<string>
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}