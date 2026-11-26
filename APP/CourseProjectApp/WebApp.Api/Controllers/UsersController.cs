using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.DatabaseLibrary.DTOs;
using WebApp.DatabaseLibrary.Services;

namespace WebApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ProfileService _profileService = new();

        public UsersController(IConfiguration configuration)
        {
            _userService = new(configuration);
        }

        [HttpPost("register")]
        public async Task<ActionResult<string>> RegisterUserAsync([FromBody] RegisterRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Заполните все обязательные поля."
                });
            }

            if (await _userService.IsUserExistsAsync(request.Username, request.Email))
                return BadRequest(new
                {
                    message = "Пользователь с таким логином или почтой уже существует."
                });

            string token = await _userService.RegisterUserAsync(request.Username, request.Email, request.Password);

            return Ok(token);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginUserAsync([FromBody] LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Введите логин и пароль."
                });
            }

            string? token = await _userService.LoginUserAsync(request.Username, request.Password);

            if (token is null)
                return Unauthorized(new
                {
                    message = "Неверный логин или пароль."
                });

            return Ok(token);
        }

        [HttpGet("top-critics")]
        public async Task<ActionResult<List<CriticDTO>>> GetTopCriticsAsync([FromQuery] int limit = 6)
        {
            var critics = await _userService.GetTopCriticsAsync(limit);
            return Ok(critics);
        }

        [HttpGet("profile/{id:int}")]
        public async Task<ActionResult<ProfileDTO>> GetProfileByIdAsync(int id)
        {
            int? currentUserId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int uid) ? uid : null;
            var profile = await _profileService.GetProfileByIdAsync(id, currentUserId);

            if (profile is null)
                return NotFound();

            return Ok(profile);
        }

        [Authorize]
        [HttpGet("profile/me")]
        public async Task<ActionResult<ProfileDTO>> GetMyProfileAsync()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
                return Unauthorized();

            var profile = await _profileService.GetProfileByIdAsync(userId, userId);
            if (profile is null)
                return NotFound();

            return Ok(profile);
        }
        [Authorize]
        [HttpPut("profile/me")]
        [HttpPost("profile/me/update")]
        public async Task<ActionResult> UpdateMyProfileAsync([FromBody] UpdateProfileDTO request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
                return Unauthorized();

            bool updated = await _profileService.UpdateProfileAsync(userId, request);
            if (!updated)
                return NotFound();

            return Ok();
        }
    }
}