// Controllers/RabbitTestController.cs
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace nigar_payment_service.Controllers;

    // RabbitMQ ile test etmek için basit bir controller
    // Postman ile /api/RabbitTest/publish endpoint’ine POST isteği atarak RabbitMQ’ya mesaj gönderebilirsiniz.
    // Mesajı JSON formatında gönderin. Örnek:
    // {
    //   "queue": "payment_succeeded",
    //   "message": { "bookingId": 1, "paymentId": 42 }
    // }
    [ApiController]
    [Route("api/[controller]")]
    public class RabbitTestController : ControllerBase
    {
        private readonly IConnectionFactory _factory;

        public RabbitTestController(IConnectionFactory factory)
        {
            _factory = factory;
        }

        // POST /api/RabbitTest/publish
        // {
        //   "queue": "payment_succeeded",
        //   "message": { "bookingId": 1, "paymentId": 42 }
        // }
        [HttpPost("publish")]
        public IActionResult Publish([FromBody] PublishRequest req)
        {
            using var connection = _factory.CreateConnection();
            using var channel    = connection.CreateModel();

            // Kuyruğu (varsa) declare et
            channel.QueueDeclare(
                queue:      req.Queue,
                durable:    true,
                exclusive:  false,
                autoDelete: false,
                arguments:  null);

            // Mesaj gövdesini JSON olarak byte[]’e çevir
            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(req.Message));

            // Yayınla
            channel.BasicPublish(
                exchange:       "",
                routingKey:     req.Queue,
                basicProperties:null,
                body:            body);

            return Ok(new { status = "published", queue = req.Queue, message = req.Message });
        }
    }

    public class PublishRequest
    {
        public string Queue { get; set; } = null!;
        public object Message { get; set; } = null!;
    }

