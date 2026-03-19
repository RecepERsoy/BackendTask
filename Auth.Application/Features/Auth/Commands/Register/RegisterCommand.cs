using MediatR;

namespace Auth.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Guid>
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}