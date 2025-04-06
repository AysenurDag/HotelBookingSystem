using auth_user_service.Data;
using auth_user_service.Messaging;
using auth_user_service.Models;
using auth_user_service.Sagas;

namespace auth_user_service.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthUserDbContext _context;
        private readonly MessagePublisher _messagePublisher;

        public AuthController(AuthUserDbContext context, MessagePublisher messagePublisher)
        {
            _context = context;
            _messagePublisher = messagePublisher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new User(dto.Email, dto.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Domain event’in yanı sıra saga’yı da çalıştırın
            var saga = new UserRegistrationSaga(_messagePublisher);
            await saga.ExecuteSaga(user);

            return Ok(new { user.Id });
        }
    }
}
