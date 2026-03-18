using MediatR;

namespace Auth.Application.Features.Auth.Commands.Login
{
    // Giriş başarılı olursa geriye JWT (Token) metni dönecek
    public class LoginCommand : IRequest<string>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}