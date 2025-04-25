using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using auth_user_service.DTOs;
using auth_user_service.Models;
using auth_user_service.Repositories;
using auth_user_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace auth_user_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IApplicationUserRepository _userRepo;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            IApplicationUserRepository userRepo,
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var existing = await _userRepo.FindByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest("Email already in use.");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                Surname = dto.Surname,
                PhoneNumber = dto.PhoneNumber
            };

            var createResult = await _userRepo.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            // Opsiyonel: birden fazla role atanabilir
            foreach (var role in dto.Roles)
                await _userManager.AddToRoleAsync(user, role);

            return Ok(new { user.Id, user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userRepo.FindByEmailAsync(dto.Email);
            if (user == null || !await _userRepo.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials.");

            // Access & Refresh token üret
            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user);

            return Ok(new
            {
                accessToken,
                refreshToken
            });
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

       
    }
}
