using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RabbitMQ.Client;
using Microsoft.Extensions.DependencyInjection;
using nigar_payment_service.Gateways;
using nigar_payment_service.Models.DTOs;
using nigar_payment_service.Models;

namespace nigar_payment_service.Tests.Gateways
{
    public class RuleBasedPaymentGatewayTests
    {
        [Fact]
        public async Task ProcessAsync_ReturnsPending()
        {
            // arrange
            var factoryMock      = new Mock<IConnectionFactory>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var gateway          = new RuleBasedPaymentGateway(factoryMock.Object, scopeFactoryMock.Object);

            var correlationId = Guid.NewGuid();
            var dto = new PaymentRequestDto(
                correlationId,
                "2L",  // bookingId
                "2L",  // userId
                100m,
                "4000000000000000",
                "12/25",
                "123");

            // act
            var response = await gateway.ProcessAsync(dto);

            // assert
            Assert.Equal(correlationId, response.CorrelationId);
            Assert.Equal(PaymentStatus.Pending, response.Status);
            Assert.Null(response.FailureReason);
        }
    }
}
