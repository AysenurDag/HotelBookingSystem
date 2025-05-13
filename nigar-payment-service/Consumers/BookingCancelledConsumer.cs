using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using nigar_payment_service.Events;
using nigar_payment_service.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace nigar_payment_service.Consumers
{
    public class BookingCancelledConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider _services;
        private readonly ILogger<BookingCancelledConsumer> _logger;

        private IConnection? _connection;
        private IModel? _channel;

        private const string ExchangeName = "booking.exchange";
        private const string QueueName = "booking.cancelled.queue";
        private const string RoutingKey = "booking.cancelled";

        public BookingCancelledConsumer(
            IConnectionFactory factory,
            IServiceProvider services,
            ILogger<BookingCancelledConsumer> logger)
        {
            _factory = factory;
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1) RabbitMQ’a bağlan/kuyrukları declare et (retry loop)
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
                    _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
                    _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
                    _channel.BasicQos(0, 1, false);

                    _logger.LogInformation("✅ Connected to RabbitMQ and declared queue '{Queue}'", QueueName);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ RabbitMQ bağlantısında hata, 5s sonra retry");
                    await Task.Delay(5_000, stoppingToken);
                }
            }

            if (_channel == null)
            {
                _logger.LogCritical("RabbitMQ kanalı oluşturulamadı, consumer başlatılamıyor.");
                return;
            }

            // 2) Mesajları dinle
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evt = JsonSerializer.Deserialize<BookingCancelledEvent>(json);
                    if (evt == null)
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    _logger.LogInformation("↔ Received BookingCancelledEvent for BookingId={BookingId}", evt.BookingId);

                    using var scope = _services.CreateScope();
                    var refundSvc = scope.ServiceProvider.GetRequiredService<IRefundService>();

                    bool ok = await refundSvc.RefundAsync(
                        long.Parse(evt.BookingId),
                        evt.Amount,
                        evt.Reason ?? "Booking Cancelled");

                    if (ok)
                        _logger.LogInformation("✅ Refund processed for BookingId={BookingId}", evt.BookingId);
                    else
                        _logger.LogError("⚠ Refund failed for BookingId={BookingId}", evt.BookingId);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, " Error processing BookingCancelledEvent");
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }
            };

            _channel.BasicConsume(QueueName, autoAck: false, consumer: consumer);

            // 3) Uygulama kapanana dek bekle
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
