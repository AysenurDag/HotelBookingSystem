using System.Security.Claims;
using auth_user_service.DTOs;
using auth_user_service.Models;
using auth_user_service.Repositories;
using auth_user_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


namespace auth_user_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IApplicationUserRepository _userRepo;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public AuthController(
            IApplicationUserRepository userRepo,
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache)
        {
            _userRepo = userRepo;
            _config = config;
            _userManager = userManager;
            _cache = cache;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto, [FromServices] EmailService emailService)
        {
            var existingUser = await _userRepo.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Email already in use");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                Surname = dto.Surname,
                PhoneNumber = dto.PhoneNumber
            };

            var result = await _userRepo.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            foreach (var role in dto.Roles)
                await _userRepo.AddToRoleAsync(user, role);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token }, Request.Scheme);

            var emailBody = $@"
            <html>
            <body>
                <p>Hello {user.Name},</p>
                <p>Please confirm your email by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm your email</a></p>
            </body>
            </html>";

            await emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);

            return Ok("Registration successful. Please check your email to confirm your account.");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, [FromServices] EmailService emailService)
        {
            var user = await _userRepo.FindByEmailAsync(dto.Email);
            if (user == null || !await _userRepo.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            var code = new Random().Next(100000, 999999).ToString();
            _cache.Set(dto.Email, code, TimeSpan.FromMinutes(5));

            await emailService.SendEmailAsync(dto.Email, "Your 2FA Code", $"Your verification code is: {code}");

            return Ok("2FA code sent to your email address.");
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FADto dto)
        {
            if (!_cache.TryGetValue(dto.Email, out string? expectedCode))
                return BadRequest("No pending 2FA request for this email");

            if (expectedCode != dto.Code)
                return Unauthorized("Invalid 2FA code");

            _cache.Remove(dto.Email);

            var user = await _userRepo.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized();

            // 🧠 Not: Burada dış token yerine kendi JWT token üretmiyorsun çünkü Entra External ID'de login işlemi frontend’ten yapılmalı.

            return Ok("2FA verified. You can now use your Azure access token.");
        }






        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _userRepo.FindByIdAsync(Guid.Parse(userId));
            if (user == null)
                return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password changed successfully.");
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout([FromServices] RedisService redisService)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                var expiration = TimeSpan.FromMinutes(60);
                redisService.AddTokenToBlacklist(token, expiration);
                return Ok("Logged out successfully.");
            }

            return BadRequest("Invalid token");
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var user = await _userRepo.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound("User not found.");

            var result = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Role '{dto.Role}' assigned to user '{user.Email}'.");
        }

        [HttpDelete("delete-user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await _userRepo.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var result = await _userRepo.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User deleted successfully.");
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userRepo.FindByIdAsync(Guid.Parse(userId));
            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return result.Succeeded
                ? Ok("Email confirmed successfully!")
                : BadRequest("Email confirmation failed.");
        }

        [HttpGet("CurrentUser")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            // 1) Email claim'i al
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("preferred_username")?.Value;
            if (email == null) return Unauthorized();

            // 2) DB'den kullanıcıyı getir
            var user = await _userRepo.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            // 3) Rolleri de DB'den çek
            var roles = await _userRepo.GetRolesAsync(user);

            // 4) Local GUID'i döndür
            return Ok(new
            {
                userId = user.Id,
                email  = user.Email,
                name   = user.Name,
                roles
            });
        }



        [HttpGet("all-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllAsync();
            return Ok(users.Select(u => new { u.Id, u.Email, u.Name, u.Surname }));
        }

        [HttpGet("test-public")]
        public IActionResult TestPublic()
        {
            return Ok("Public endpoint erişildi");
        }

    }
}