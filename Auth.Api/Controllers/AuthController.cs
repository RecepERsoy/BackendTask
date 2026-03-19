using Auth.Application.Features.Auth.Commands.Login;
using Auth.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers
{
    /// <summary>
    /// Kimlik doğrulama ve yetkilendirme (Authentication & Authorization) süreçlerini yöneten Controller.
    /// İş mantığı (Business Logic) API katmanından soyutlanarak MediatR üzerinden Application katmanına devredilmiştir.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Sisteme yeni bir kullanıcı kaydeder.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Auth.Application.Features.Auth.Commands.Register.RegisterCommand command)
        {
            try
            {
                // İşlem başarılı olursa result artık yeni kullanıcının ID'si (Guid) olacak.
                var result = await _mediator.Send(command);

                // Başarılı kayıtta ID'yi geri dönüyoruz.
                return Ok(new { Message = "Kullanıcı başarıyla oluşturuldu.", UserId = result });
            }
            catch (Exception ex)
            {
                // Eğer Handler'da bir hata fırlatıldıysa (örn: Şifre çok kısa, Email kullanılıyor vs.)
                // Kod direkt buraya düşer ve hatayı 400 Bad Request olarak döndürür.
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Kullanıcı kimlik doğrulamasını gerçekleştirir.
        /// Başarılı giriş durumunda oturum sürekliliği için Access Token ve Refresh Token çifti döndürür.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            try
            {
                // İş mantığından AuthResponseDto (AccessToken ve RefreshToken) döner
                var response = await _mediator.Send(command);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Application katmanında fırlatılan doğrulama hatalarını (örn: Yanlış şifre) yakalar
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] Auth.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand command)
        {
            try
            {
                var response = await _mediator.Send(command);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}