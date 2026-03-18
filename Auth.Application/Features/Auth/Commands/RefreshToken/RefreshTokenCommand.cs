using Auth.Application.Features.Auth.Commands.Login;
using MediatR;

namespace Auth.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<AuthResponseDto>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}