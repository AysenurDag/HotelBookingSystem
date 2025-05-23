using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace authuserservice.Tests.Tests
{
    public class UserTests
    {
        [Fact]
        public void CreateUser_Should_Add_UserRegisteredEvent()
        {
            // Arrange & Act
            var user = new User("test@example.com", "hashedPassword");

            // Assert
            Assert.NotEmpty(user.DomainEvents);
            Assert.IsType<UserRegisteredEvent>(user.DomainEvents.First());
        }
    }
}
