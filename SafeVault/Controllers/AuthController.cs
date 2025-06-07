using Microsoft.AspNetCore.Mvc;
using SafeVault.Models;
using SafeVault.Services;
using Microsoft.AspNetCore.Authorization;

namespace SafeVault.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, token, role) = await _authService.LoginUser(model.Username, model.Password);

            if (success)
            {
                _logger.LogInformation("User {Username} logged in successfully", model.Username);
                return Ok(new { token, role });
            }

            _logger.LogWarning("Failed login attempt for user {Username}", model.Username);
            return Unauthorized("Invalid username or password");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _authService.RegisterUser(model);

            if (success)
            {
                _logger.LogInformation("User {Username} registered successfully", model.Username);
                return Ok(new { message });
            }

            _logger.LogWarning("Failed registration attempt for user {Username}: {Message}", model.Username, message);
            return BadRequest(message);
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Authentication works!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok("You are an admin!");
        }
    }
}
