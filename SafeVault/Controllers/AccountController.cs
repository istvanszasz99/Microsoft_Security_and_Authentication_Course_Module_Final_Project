using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SafeVault.Models;

namespace SafeVault.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // This is just a view that will clear the token on the client side
            return View();
        }
    }
}
