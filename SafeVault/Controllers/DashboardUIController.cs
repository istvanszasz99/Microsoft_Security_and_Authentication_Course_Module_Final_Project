using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SafeVault.Controllers
{
    [Authorize] // Require authentication for all actions in this controller
    public class DashboardUIController : Controller
    {
        private readonly ILogger<DashboardUIController> _logger;

        public DashboardUIController(ILogger<DashboardUIController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("~/Views/Dashboard/Index.cshtml");
        }
    }
}
