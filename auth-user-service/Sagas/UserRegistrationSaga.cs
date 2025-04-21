using auth_user_service.Messaging;
using auth_user_service.Models;

namespace auth_user_service.Sagas
{
    public class UserRegistrationSaga
    {
        private readonly IMessagePublisher _messagePublisher;
        public UserRegistrationSaga(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task ExecuteSaga(User user)
        {
            try
            {

                // Adım 2: Hoş geldin e-posta gönderimi (örnek, message queue kullanımı)
                var emailSent = await SendWelcomeEmail(user);
                if (!emailSent)
                    throw new Exception("Email gönderimi başarısız.");

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
            _messagePublisher.Publish($"SendWelcomeEmail:{user.Email}");
            return Task.FromResult(true);
        }

        private Task<bool> InitializeBookingProfile(User user)
        {
            _messagePublisher.Publish($"InitializeBookingProfile:{user.Id}");
            return Task.FromResult(true);
        }

        private Task CompensateUserRegistration(User user)
        {
            // Başarısızsa,kaydı iptal et
            _messagePublisher.Publish($"CompensateRegistration:{user.Id}");
            return Task.CompletedTask;
        }
    }
}
