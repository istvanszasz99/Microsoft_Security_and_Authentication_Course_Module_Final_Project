using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SafeVault.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetDashboard()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            _logger.LogInformation("User {Username} with role {Role} accessed dashboard", username, role);
            
            // Basic data for all users
            var dashboardData = new
            {
                username = username,
                role = role,
                lastLogin = DateTime.UtcNow,
                message = "Welcome to your dashboard"
            };
            
            return Ok(dashboardData);
        }

        [HttpGet("profile")]
        public IActionResult GetUserProfile()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            // In a real app, you'd fetch data from the database for this user
            var profileData = new
            {
                username = username,
                email = $"{username}@example.com", // Placeholder
                created = DateTime.UtcNow.AddDays(-30), // Placeholder
                lastActive = DateTime.UtcNow
            };
            
            return Ok(profileData);
        }
        
        // Special endpoints that only admins can access,
        // even within this controller that's accessible to all authenticated users
        [Authorize(Roles = "Admin")]
        [HttpGet("advanced")]
        public IActionResult GetAdvancedData()
        {
            _logger.LogInformation("Admin accessed advanced dashboard data");
            
            var advancedData = new
            {
                systemStatus = "Online",
                activeUsers = 42,
                storageUsage = 68.5,
                criticalAlerts = 0
            };
            
            return Ok(advancedData);
        }
    }
}
