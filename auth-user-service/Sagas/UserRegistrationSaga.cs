using auth_user_service.Messaging;
using auth_user_service.Models;

namespace auth_user_service.Sagas
{
    public class UserRegistrationSaga
    {
        private readonly MessagePublisher _messagePublisher;

        public UserRegistrationSaga(MessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task ExecuteSaga(User user)
        {
            try
            {
                // Adım 1: Kullanıcı kaydı yapılmış ve domain event tetiklenmiş durumda.

                // Adım 2: Hoş geldin e-posta gönderimi (örnek, message queue kullanımı)
                var emailSent = await SendWelcomeEmail(user);
                if (!emailSent)
                    throw new Exception("Email gönderimi başarısız.");

                // Adım 3: Booking servisine başlangıç ayarlarının yapılması
                var bookingInitialized = await InitializeBookingProfile(user);
                if (!bookingInitialized)
                    throw new Exception("Booking servisine entegrasyon başarısız.");

                // Saga başarılı, tüm adımlar tamamlandı.
            }
            catch (Exception)
            {
                // Geri alma (compensation) işlemleri
                await CompensateUserRegistration(user);
            }
        }

        private Task<bool> SendWelcomeEmail(User user)
        {
            // Mesaj kuyruğu üzerinden hoş geldin email mesajı gönderimi örneği
            _messagePublisher.Publish($"SendWelcomeEmail:{user.Email}");
            return Task.FromResult(true);
        }

        private Task<bool> InitializeBookingProfile(User user)
        {
            // Booking servisine gerekli mesajı gönderme işlemi
            _messagePublisher.Publish($"InitializeBookingProfile:{user.Id}");
            return Task.FromResult(true);
        }

        private Task CompensateUserRegistration(User user)
        {
            // Başarısızlık durumunda, kullanıcı kaydının iptal edilmesi gibi işlemler
            _messagePublisher.Publish($"CompensateRegistration:{user.Id}");
            return Task.CompletedTask;
        }
    }
}
