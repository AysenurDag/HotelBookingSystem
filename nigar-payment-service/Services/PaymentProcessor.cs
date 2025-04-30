using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.Events;
using nigar_payment_service.Models;
using nigar_payment_service.Services;



namespace nigar_payment_service.Services;

public class PaymentProcessor : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "10.47.7.151" }; // Sunucu IP
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "booking.created.queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var booking = JsonSerializer.Deserialize<BookingCreatedEvent>(message);

            Console.WriteLine($"📩 Booking CreatedEvent received. Booking ID: {booking.BookingId}");

            var success = new Random().Next(0, 2) == 0;

            if (success)
            {
                Console.WriteLine($"💳 Payment succeeded for Booking ID: {booking.BookingId}");
                var successEvent = new PaymentSucceededEvent
                {
                 
                };
                PublishEvent(successEvent, "payment_succeeded", channel);
            }
            else
            {
                Console.WriteLine($"❌ Payment failed for Booking ID: {booking.BookingId}");
                var failedEvent = new PaymentFailedEvent
                {
                    BookingId = booking.BookingId,
                    Reason = "Payment processing failed."
                };
                PublishEvent(failedEvent, "payment_failed", channel);
            }
        };

        channel.BasicConsume(queue: "booking.created.queue", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    private void PublishEvent<T>(T @event, string queueName, IModel channel)
    {
        channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }
}
