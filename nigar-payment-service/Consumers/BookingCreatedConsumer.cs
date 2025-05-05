 using System.Text;
using System.Text.Json; 
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Gateways;
using nigar_payment_service.Models;
using nigar_payment_service.Models.DTOs;

namespace nigar_payment_service.Consumers
{
    public class BookingCreatedConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider   _services;
        private readonly IPaymentGateway    _gateway;

        private const string BookingExchange    = "booking.exchange";
        private const string BookingQueue       = "booking.created.queue";
        private const string BookingRoutingKey  = "booking.created";

        private const string PaymentSuccessQueue = "payment.success.queue";
        private const string PaymentFailedQueue  = "payment.failed.queue";

        public BookingCreatedConsumer(
            IConnectionFactory factory,
            IServiceProvider   services,
            IPaymentGateway    gateway)
        {
            _factory  = factory;
            _services = services;
            _gateway  = gateway;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1) RabbitMQ’a bağlanana kadar retry
            IConnection connection = null!;
            IModel      channel    = null!;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    connection = _factory.CreateConnection();
                    channel    = connection.CreateModel();

                    // 2) Exchange ve Queue declare
                    channel.ExchangeDeclare(
                        exchange:   BookingExchange,
                        type:       ExchangeType.Topic,
                        durable:    true,
                        autoDelete: false,
                        arguments:  null);

                    channel.QueueDeclare(
                        queue:      BookingQueue,
                        durable:    true,
                        exclusive:  false,
                        autoDelete: false,
                        arguments:  null);

                    // Yeni eklenen kuyruk tanımları
                    channel.QueueDeclare(
                        queue:      PaymentSuccessQueue,
                        durable:    true,
                        exclusive:  false,
                        autoDelete: false,
                        arguments:  null);

                    channel.QueueDeclare(
                        queue:      PaymentFailedQueue,
                        durable:    true,
                        exclusive:  false,
                        autoDelete: false,
                        arguments:  null);

                    // 3) Bind
                    channel.QueueBind(
                        queue:      BookingQueue,
                        exchange:   BookingExchange,
                        routingKey: BookingRoutingKey,
                        arguments:  null);

                    Console.WriteLine(
                      $"✅ Listening on '{BookingExchange}' → '{BookingQueue}' ({BookingRoutingKey})");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                      $"❌ RabbitMQ bağlantısında hata: {ex.Message}. 5s sonra retry...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            if (channel == null) return;

            var consumer = new AsyncEventingBasicConsumer(channel);
          consumer.Received += async (sender, ea) =>
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested) return;

                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evt  = JsonSerializer.Deserialize<BookingCreatedEvent>(json);
                    if (evt == null)
                    {
                        Console.WriteLine("❌ Geçersiz BookingCreatedEvent, ack’ediliyor.");
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    Console.WriteLine($"📩 BookingCreatedEvent received: {json}");

                    // 1) DB’ye Pending kaydı ekle
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                    var payment = new Payment
                    {
                        BookingId     = evt.BookingId,
                        CustomerId    = evt.UserId,
                        Amount        = evt.TotalAmount,
                        Status        = PaymentStatus.Pending,
                        CreatedAt     = DateTime.UtcNow,
                        CorrelationId = Guid.NewGuid(),
                        CardLast4     = "0000"
                    };
                    Console.WriteLine($"➡️ DB kaydı öncesi: BookingId = {evt.BookingId}");
                    db.Payments.Add(payment);
                    await db.SaveChangesAsync();
                    Console.WriteLine($"✅ DB kaydı başarılı: PaymentId = {payment.Id}");
                    // 2) Gerçek ya da dummy gateway çağrısı
                    var dto = new PaymentRequestDto(
                        payment.CorrelationId,
                        evt.BookingId,
                        evt.UserId,
                        evt.TotalAmount,
                        "0000000000000000", "01/30", "123");
                    bool success = false;
                    string? failureReason = null;
                    try
                    {
                        Console.WriteLine($"📞 Gateway çağrısı yapılıyor: CorrelationId = {payment.CorrelationId}");

                        var gatewayResponse = await _gateway.ProcessAsync(dto);
                        success = gatewayResponse.Status == PaymentStatus.Success;
                        
                        Console.WriteLine($"↩️ Gateway yanıtı alındı  Success: Status = {gatewayResponse.Status}");

                        failureReason = gatewayResponse.FailureReason;
                        Console.WriteLine($"↩️ Gateway yanıtı alındı  FailureReason: Status = {gatewayResponse.Status}");

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Gateway error: {ex.Message}");
                        failureReason = ex.Message;
                    }

                    // 3) Sonuca göre event publish
                    object resultEvent;
                    var targetQueue = "";

                    if (success)
                    {
                        resultEvent = new PaymentSucceededEvent
                        {
                            BookingId = evt.BookingId,
                            PaymentId = payment.Id
                        };
                        targetQueue = PaymentSuccessQueue;
                    }
                    else
                    {
                        resultEvent = new PaymentFailedEvent
                        {
                            BookingId = evt.BookingId,
                            PaymentId = payment.Id,
                            Reason = failureReason ?? "Ödeme işlemi başarısız oldu."
                        };
                        targetQueue = PaymentFailedQueue;
                    }

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(resultEvent));

                    channel.BasicPublish(
                        exchange:    "",                       // default exchange
                        routingKey:  targetQueue,
                        basicProperties: null,
                        body:         body);

                    Console.WriteLine(
                      $"📤 Published {(success?"Succeeded":"Failed")}Event to '{targetQueue}': " +
                      Encoding.UTF8.GetString(body));

                    // 4) asıl mesajı ack’le
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"🔥 Bir hata oluştu: {ex}");
                    // Hata durumunda mesajı tekrar kuyruğa atmak yerine (sonsuz döngüye yol açabilir),
                    // loglayıp geçici bir çözüm olarak ack'leyebiliriz veya DLQ'ya gönderebiliriz.
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            channel.BasicConsume(
                queue:    BookingQueue,
                autoAck:  false,
                consumer: consumer);

            // Processu canlı tut
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}