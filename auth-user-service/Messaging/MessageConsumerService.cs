using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


namespace auth_user_service.Messaging

{
    public class MessageConsumerService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;

        public MessageConsumerService()
        {
            var factory = new ConnectionFactory() { HostName = "10.47.7.151" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "userQueue", durable: false, exclusive: false, autoDelete: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received message: {message}");
            };

            _channel.BasicConsume(queue: "userQueue", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }
    }
}
