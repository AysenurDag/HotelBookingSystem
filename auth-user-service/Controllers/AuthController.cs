using auth_user_service.Data;
using auth_user_service.Messaging;
using auth_user_service.Models;
using auth_user_service.Sagas;
using Microsoft.AspNetCore.Mvc;
using auth_user_service.DTOs;
using auth_user_service.Repositories;



namespace auth_user_service.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly RabbitMqPublisher _messagePublisher;

        public AuthController(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            RabbitMqPublisher messagePublisher)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _messagePublisher = messagePublisher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new User(
                dto.Name,
                dto.Surname,
                dto.Email,
                dto.PhoneNumber,
                dto.Password
            );

            await _userRepo.AddAsync(user);

            var rolesToAssign = dto.Roles?.Any() == true
                ? dto.Roles
                : new List<string> { "ROLE_USER" };

            foreach (var roleName in rolesToAssign)
            {
                var role = await _roleRepo.GetByNameAsync(roleName);
                if (role != null)
                {
                    await _userRepo.AddRoleAsync(user.Id, role.Id);
                }
                else
                {
                }
            }

            var saga = new UserRegistrationSaga(_messagePublisher);
            await saga.ExecuteSaga(user);

            return Ok(new { user.Id });
        }
    }
}
