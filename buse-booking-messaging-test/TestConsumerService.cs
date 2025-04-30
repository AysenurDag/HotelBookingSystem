using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System;


public class TestConsumerService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public TestConsumerService()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        ListenToBookingMessages();
        SendTestMessagesToBooking();
    }

    private void ListenToBookingMessages()
    {
        string[] queuesToConsume = {
            "booking.created.queue",
            "booking.cancelled.queue",
            "booking.confirmed.queue"
        };

        foreach (var queue in queuesToConsume)
        {
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[📥] Received from {queue}: {message}");
            };

            _channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
            Console.WriteLine($"[🔍] Listening on {queue}...");
        }
    }

    private void SendTestMessagesToBooking()
    {
        Console.WriteLine(">>> Sending test events to Booking Service...");

        // PaymentSucceededEvent
        var paymentSuccess = new
        {
            bookingId = "1",
            paymentId = "PAY-001"
        };
        PublishMessage("payment.success.queue", paymentSuccess);

        // PaymentFailedEvent
        var paymentFailed = new
        {
            bookingId = "2"
        };
        PublishMessage("payment.failed.queue", paymentFailed);

        // RoomAvailabilityFailedEvent
        var roomFailed = new
        {
            bookingId = "3",
            roomId = "R101",
            reason = "Room is not available"
        };
        PublishMessage("room.failed.queue", roomFailed);
    }

    private void PublishMessage(string queueName, object payload)
    {
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        Console.WriteLine($"[📤] Sent to {queueName}: {json}");
    }

    public void Close()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
