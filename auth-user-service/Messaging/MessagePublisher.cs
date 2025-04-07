using RabbitMQ.Client;
using System.Text;


namespace auth_user_service.Messaging
{
    public class MessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessagePublisher()
        {
            var factory = new ConnectionFactory() { HostName = "10.47.7.151" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "userQueue", durable: false, exclusive: false, autoDelete: false);
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "userQueue", basicProperties: null, body: body);
        }
    }
}
