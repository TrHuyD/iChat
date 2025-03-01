using iChat.App.Services.User;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Mvc;

namespace iChat.App.Controllers.User
{
    [Route("api/[controller]")]
    public class AuthController :Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        [Route("/login/summit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View();
            try
            {
                var response = await _authService.LoginAsync(request);
                if (!response.Success)
                {
                    ViewData["ErrorMessage"]= response.Message;
                    return View();
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        [HttpGet("login")]
        [Route("/login")]
        public IActionResult Login()
        {
            return View("Login");
        }
    }
}
