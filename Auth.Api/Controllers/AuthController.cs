using Auth.Application.Features.Auth.Commands.Login;
using Auth.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace Auth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        // MediatR içeri alıyoruz
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            // Kullanıcıdan gelen "Kayıt Ol" isteğini (şifre, email vs.) alıp MediatR'a veriyoruz.
            // O gidip  RegisterCommandHandler'ı çalıştıracak.
            var result = await _mediator.Send(command);

            // Eğer hata mesajı dönerse 400 Bad Request dön
            if (result.StartsWith("Hata"))
            {
                return BadRequest(result);
            }

            // Başarılıysa 200 OK ile mesajı dön
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);

            // Eğer hata mesajıyla başlıyorsa 400 Bad Request dön
            if (result.StartsWith("Hata"))
            {
                return BadRequest(result);
            }

            // Başarılıysa Token dön
            return Ok(new { Token = result });
        }
    }
}