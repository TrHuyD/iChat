using iChat.BackEnd.Services.Users.Auth;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers
{
    [Route("")]
    public class UserController : Controller
    {
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpGet()]
        [Authorize]
        public IActionResult Home()
        {
            return View();
        }
        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                await _authService.RegisterAsync(request);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(request);
            }
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                await _authService.LoginAsync(request);
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(request);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            return View();
        }
    }
}
