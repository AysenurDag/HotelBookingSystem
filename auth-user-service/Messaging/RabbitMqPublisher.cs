using System;                     // ← IDisposable burada
using System.Text;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;





namespace auth_user_service.Messaging
{
    public class RabbitMqPublisher : IMessagePublisher, IDisposable
    {
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly string _queueName;

        public RabbitMqPublisher(IConfiguration cfg)
        {
            var rmq = cfg.GetSection("RabbitMq");
            var factory = new ConnectionFactory() { HostName = rmq["HostName"] };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _queueName = rmq["QueueName"];
            _channel.QueueDeclare(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: null,
                body: body
            );
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
