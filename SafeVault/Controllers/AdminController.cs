using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafeVault.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            _logger.LogInformation("Admin accessed dashboard");
            return Ok(new { message = "Welcome to the Admin Dashboard", date = DateTime.UtcNow });
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            // In a real app, you would get this data from your database
            var users = new List<object>
            {
                new { id = 1, username = "admin", email = "admin@safevault.com", role = "Admin" },
                new { id = 2, username = "user", email = "user@safevault.com", role = "User" }
            };

            _logger.LogInformation("Admin retrieved user list");
            return Ok(users);
        }

        [HttpPost("adduser")]
        public IActionResult AddUser([FromBody] object userInfo)
        {
            _logger.LogInformation("Admin attempted to add a new user");
            // In a real app, you'd implement the user creation logic
            return Ok(new { message = "User added successfully", user = userInfo });
        }
    }
}
