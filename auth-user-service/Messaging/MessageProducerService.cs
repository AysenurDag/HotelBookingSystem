using RabbitMQ.Client;
using System;
using System.Text;
namespace auth_user_service.Messaging
{
    

    public class MessageProducerService
    {
        private IConnection _connection;
        private IModel _channel;

        public MessageProducerService()
        {
            //10.47.7.151
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "userQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "userQueue", basicProperties: null, body: body);
            Console.WriteLine("Gönderilen mesaj: " + message);
        }

        public void Close()
        {
            _channel.Close();
            _connection.Close();
        }
    }

}
