using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace authuserservice.Tests.Tests
{
    public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterEndpoint_Should_Return_OK()
        {
            // Örneğin, RegisterDto tanımlı ise
            var registerDto = new { Email = "test@example.com", Password = "password" };
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            response.EnsureSuccessStatusCode();
        }
    }
}
