using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;   
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.Events;
using nigar_payment_service.Services;

namespace nigar_payment_service.Consumers
{
    public class BookingCancelledConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider   _services;
        private IConnection? _connection;
        private IModel?      _channel;

        const string ExchangeName        = "booking.exchange";
        const string CancelledQueue      = "booking.cancelled.queue";
        const string CancelledRoutingKey = "booking.cancelled";

        public BookingCancelledConsumer(
            IConnectionFactory factory,
            IServiceProvider services)
        {
            _factory  = factory;
            _services = services;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _factory.CreateConnection();
            _channel    = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(CancelledQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(CancelledQueue, ExchangeName, CancelledRoutingKey);
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel!);
            consumer.Received += async (_, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt  = JsonSerializer.Deserialize<BookingCancelledEvent>(json);
                if (evt == null)
                {
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                try
                {
                    Console.WriteLine($"↔ Received booking.cancelled: {evt.BookingId}");

                    
                    using var scope = _services.CreateScope();
                    var refundSvc = scope.ServiceProvider.GetRequiredService<IRefundService>();

                    // refund işlemi ve event publish
                    await refundSvc.RefundAsync(
                        paymentId: long.Parse(evt.BookingId),
                        amount:    evt.Amount,
                        reason:    evt.Reason);

                    _channel!.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Error processing refund: {ex}");
                    _channel!.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };

            _channel!.BasicConsume(
                queue:    CancelledQueue,
                autoAck:  false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
