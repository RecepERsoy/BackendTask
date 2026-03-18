using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;

namespace Auth.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
    {
        private readonly IAuthRepository _authRepository;

        // Repository'iyi içeri alıyoruz
        public RegisterCommandHandler(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // KONTROL: Bu e-posta adresiyle daha önce kayıt olunmuş mu?
            var existingUser = await _authRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return "Hata: Bu e-posta adresi zaten kullanılıyor!";
            }


            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // YENİ KULLANICI: Şifrelenmiş parola ile kullanıcıyı oluştur
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = "User",
                CreatedDate = DateTime.UtcNow
            };

            // KAYDET:Repositorye bu yeni kullanıcıyı SQL'e kaydetmesini söyle
            await _authRepository.AddUserAsync(newUser);

            return "Kayıt işlemi başarıyla tamamlandı!";
        }
    }
}