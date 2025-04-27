using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using auth_user_service.DTOs;
using auth_user_service.Models;
using auth_user_service.Repositories;
using auth_user_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace auth_user_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IApplicationUserRepository _userRepo;
        private static Dictionary<string, string> _pending2FACodes = new();
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            IApplicationUserRepository userRepo,
            ITokenService tokenService,
            IConfiguration config,
            UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _config = config;
            _tokenService = tokenService;
            _userManager = userManager;
            
           
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

            // Kullanıcıya roller ataması
            foreach (var role in dto.Roles)
                await _userRepo.AddToRoleAsync(user, role);

            // Email Confirmation Token 
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Action(
                action: "ConfirmEmail",
                controller: "Auth",
                values: new { userId = user.Id, token = token },
                protocol: Request.Scheme
            );

            // Email body'yi düzgün HTML formatında hazırla
            var emailBody = $@"
            <html>
            <body>
                <p>Hello {user.Name},</p>
                <p>Please confirm your email by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm your email</a></p>
            </body>
            </html>";

            // HTML body'yi gönder
            await emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);



            return Ok("Registration successful. Please check your email to confirm your account.");
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userRepo.FindByIdAsync(Guid.Parse(userId));
            if (user == null)
                return NotFound("User not found");

            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                return Ok("Email confirmed successfully!");
            else
                return BadRequest("Email confirmation failed.");
        }




        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, [FromServices] EmailService emailService)
        {
            var user = await _userRepo.FindByEmailAsync(dto.Email);
            if (user == null || !await _userRepo.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            var code = new Random().Next(100000, 999999).ToString();
            _pending2FACodes[dto.Email] = code;

            await emailService.SendEmailAsync(dto.Email, "Your 2FA Code", $"Your verification code is: {code}");

            return Ok("2FA code sent to your email address.");
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA(string email, string code , [FromServices] IConfiguration config)
        {
            if (!_pending2FACodes.ContainsKey(email))
                return BadRequest("No pending 2FA request for this email");

            if (_pending2FACodes[email] != code)
                return Unauthorized("Invalid 2FA code");

            _pending2FACodes.Remove(email);

            var user = await _userRepo.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized();

            // Token üret
            var roles = await _userRepo.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email)
    };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString });
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var rt = await _tokenService.GetValidRefreshTokenAsync(dto.RefreshToken);
            if (rt == null) return Unauthorized("Invalid refresh token.");

            var user = await _userManager.FindByIdAsync(rt.UserId);
            if (user == null) return Unauthorized("User not found.");

            await _tokenService.MarkRefreshTokenAsUsed(rt);
            var (newAccess, newRefresh) = await _tokenService.GenerateTokensAsync(user);

            return Ok(new
            {
                accessToken = newAccess,
                refreshToken = newRefresh
            });
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

            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password changed successfully");
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout([FromServices] RedisService redisService)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                var expiration = TimeSpan.FromMinutes(60); // Token süresi kadar blacklist'te dursun
                redisService.AddTokenToBlacklist(token, expiration);
                return Ok("Logged out successfully");
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

            // IRoleRepository yerine doğrudan UserManager üzerinden atıyoruz:
            var result = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Role '{dto.Role}' assigned to user '{user.Email}'.");
        }

        [HttpGet("CurrentUser")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value);

            return Ok(new { id, email, roles });
        }

        [HttpGet("all-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllAsync();
            return Ok(users.Select(u => new
            {
                u.Id,
                u.Email,
                u.Name,
                u.Surname
            }));
        }




    }
}
