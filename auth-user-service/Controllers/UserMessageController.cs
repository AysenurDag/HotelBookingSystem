using auth_user_service.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace auth_user_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserMessageController : ControllerBase
    {
        private readonly RabbitMqPublisher _publisher;

        public UserMessageController(RabbitMqPublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] string message)
        {
            _publisher.Publish(message);
            return Ok("Mesaj başarıyla RabbitMQ'ya gönderildi.");
        }
    }
}

