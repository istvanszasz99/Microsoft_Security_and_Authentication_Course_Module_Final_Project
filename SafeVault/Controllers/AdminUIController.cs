using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SafeVault.Controllers
{
    [Authorize(Roles = "Admin")] // Require Admin role for all actions
    public class AdminUIController : Controller
    {
        private readonly ILogger<AdminUIController> _logger;

        public AdminUIController(ILogger<AdminUIController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/Index.cshtml");
        }
    }
}
