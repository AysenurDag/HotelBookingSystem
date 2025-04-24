using auth_user_service.Messaging;
using auth_user_service.Models;

namespace auth_user_service.Sagas
{
    public class UserRegistrationSaga
    {
        private readonly IMessagePublisher _messagePublisher;

        public UserRegistrationSaga(IMessagePublisher messagePublisher)
            => _messagePublisher = messagePublisher;

        public async Task ExecuteSaga(ApplicationUser user)
        {
            try
            {
                var emailSent = await SendWelcomeEmail(user);
                if (!emailSent)
                    throw new Exception("E-posta gönderimi başarısız.");

                var bookingInitialized = await InitializeBookingProfile(user);
                if (!bookingInitialized)
                    throw new Exception("Booking servisi entegrasyonu başarısız.");

                // Tüm adımlar başarılıysa harekete gerek yok.
            }
            catch (Exception)
            {
                await CompensateUserRegistration(user);
            }
        }

        private Task<bool> SendWelcomeEmail(ApplicationUser user)
        {
            _messagePublisher.Publish($"SendWelcomeEmail:{user.Email}");
            return Task.FromResult(true);
        }

        private Task<bool> InitializeBookingProfile(ApplicationUser user)
        {
            _messagePublisher.Publish($"InitializeBookingProfile:{user.Id}");
            return Task.FromResult(true);
        }

        private Task CompensateUserRegistration(ApplicationUser user)
        {
            _messagePublisher.Publish($"CompensateRegistration:{user.Id}");
            return Task.CompletedTask;
        }
    }
}
