using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using nigar_payment_service.Services; // Make sure this and other namespaces are correct
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using nigar_payment_service.Events;

namespace nigar_payment_service.Consumers
{
    public class BookingCancelledConsumer : BackgroundService // Changed from IHostedService to BackgroundService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IServiceProvider _serviceProvider; // Use IServiceProvider
        private readonly ILogger<BookingCancelledConsumer> _logger;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName = "booking.cancelled.queue";

        public BookingCancelledConsumer(
            IConnectionFactory connectionFactory,
            IServiceProvider serviceProvider, // Inject IServiceProvider
            ILogger<BookingCancelledConsumer> logger)
        {
            _connectionFactory = connectionFactory;
            _serviceProvider = serviceProvider; // Store it
            _logger = logger;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
             try
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                 _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            }
            catch(Exception ex)
            {
                 _logger.LogError(ex, "Error initializing RabbitMQ");
                 // Consider if you want to throw the exception or retry.  For now, log and continue.
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ channel is not initialized. Consumer will not start.");
                return; // Exit if RabbitMQ is not set up.
            }

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var bookingCancelledEvent = JsonSerializer.Deserialize<BookingCancelledEvent>(message);

                    if (bookingCancelledEvent != null)
                    {
                        _logger.LogInformation($"Received BookingCancelledEvent for BookingId: {bookingCancelledEvent.BookingId}");

                        // *** IMPORTANT: Create a scope here! ***
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            // Resolve the RefundService from the scope
                            var refundService = scope.ServiceProvider.GetRequiredService<IRefundService>();
                            // Perform the refund operation
                              await refundService.RefundAsync(long.Parse(bookingCancelledEvent.BookingId), bookingCancelledEvent.Amount, "Booking Cancelled");
                            // Since the return type of RefundAsync is now bool, adjust accordingly
                            bool refundSuccessful = await refundService.RefundAsync(long.Parse(bookingCancelledEvent.BookingId), bookingCancelledEvent.Amount, "Booking Cancelled");

                            if (refundSuccessful)
                            {
                                 _logger.LogInformation($"Refund processed successfully for BookingId: {bookingCancelledEvent.BookingId}");
                            }
                            else
                            {
                                _logger.LogError($"Refund failed for BookingId: {bookingCancelledEvent.BookingId}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Received null BookingCancelledEvent message.");
                    }
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing BookingCancelledEvent");
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true); // or false, depending on your retry policy
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: false, // Manual ACK
                                 consumer: consumer);

            _logger.LogInformation($"Consumer started for queue: {_queueName}");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        public override void Dispose()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
            }
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
            base.Dispose();
        }
    }
}
