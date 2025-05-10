using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;
using nigar_payment_service.Consumers;
using nigar_payment_service.DbContext;
using nigar_payment_service.Events;
using nigar_payment_service.Gateways;
using nigar_payment_service.Models;
using nigar_payment_service.Models.DTOs;

namespace nigar_payment_service.Tests.Consumers
{
    public class BookingCreatedConsumerTests
    {
        private IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddDbContext<PaymentDbContext>(opt =>
                opt.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task BookingCreatedEvent_IsHandled_PaymentCreated_And_Acked()
        {
            // ARRANGE
            var svcProvider = BuildServiceProvider();

            // 1) mock RabbitMQ connection/channel
            var channelMock    = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var factoryMock = new Mock<IConnectionFactory>();
            factoryMock.Setup(f => f.CreateConnection())
                       .Returns(connectionMock.Object);

            // 2) mock gateway
            var gatewayMock = new Mock<IPaymentGateway>();
            gatewayMock
              .Setup(g => g.ProcessAsync(It.IsAny<PaymentRequestDto>()))
              .ReturnsAsync(new GatewayResponse(
                 CorrelationId: Guid.NewGuid(),
                 Status: PaymentStatus.Success
                 
              ));

            // 3) create consumer
            var consumer = new BookingCreatedConsumer(
                factoryMock.Object,
                svcProvider,
                gatewayMock.Object
            );

            // start the hosted service so it declares queue & registers consumer
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await consumer.StartAsync(cts.Token);
            // give it a moment to call QueueDeclare/BasicConsume
            await Task.Delay(100, CancellationToken.None);

            // verify queue setup
            channelMock.Verify(c => c.QueueDeclare(
                "booking.created.queue", true, false, false, null), Times.Once);
            channelMock.Verify(c => c.BasicConsume(
                "booking.created.queue", false, It.IsAny<AsyncEventingBasicConsumer>()),
                Times.Once);

            // ACT: simulate delivery
            var evt = new BookingCreatedEvent
            {
                BookingId   = "123",
                UserId      = "456",
                TotalAmount = 78.90m
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));

            // pull out the consumer instance that was registered
            var registeredConsumer = channelMock.Invocations
                .Where(i => i.Method.Name == nameof(channelMock.Object.BasicConsume))
                .Select(i => i.Arguments[2])
                .OfType<AsyncEventingBasicConsumer>()
                .Single();

            await registeredConsumer.HandleBasicDeliver(
                consumerTag: "ctag",
                deliveryTag: 42UL,
                redelivered: false,
                exchange:    "booking.exchange",
                routingKey:  "booking.created",
                properties:  null,
                body:        new ReadOnlyMemory<byte>(body)
            );

            // ASSERT gateway called once with correct DTO
            gatewayMock.Verify(g => g.ProcessAsync(It.Is<PaymentRequestDto>(dto =>
                dto.BookingId   == "123" &&
                dto.CustomerId  == "456" &&
                dto.Amount      == 78.90m
            )), Times.Once);

            // assert we ack'ed the message
            channelMock.Verify(c => c.BasicAck(42UL, false), Times.Once);

            // assert DB record
            using var scope = svcProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            var payment = await db.Payments.SingleAsync(p => p.BookingId == "123");

            Assert.Equal(PaymentStatus.Pending, payment.Status);
            Assert.Equal(78.90m, payment.Amount);

            // stop the hosted service
            await consumer.StopAsync(CancellationToken.None);
        }
    }
}
