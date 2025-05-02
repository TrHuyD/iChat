using Microsoft.AspNetCore.Mvc;
using iChat.DTOs.Users.Auth;
using iChat.BackEnd.Services.Users.Auth.Auth0;
using System.Threading.Tasks;
using iChat.BackEnd.Services.Users.Auth;

namespace iChat.BackEnd.Controllers
{
    [Route("test")]
    public class AuthTestController : Controller
    {
        private readonly  IRegisterService _registerService;

        public AuthTestController(IRegisterService registerService)
        {
            _registerService = registerService;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = await _registerService.CreateUserAsync(request);

            if (!result.Success)
            {
                ModelState.AddModelError("", $"Error: {result.ErrorMessage}");
                return View(request);
            }

            ViewBag.Message = "User created successfully!";
            return View();
        }
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request, [FromServices] ILoginService loginService)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = await loginService.LoginAsync(request);

            if (!result.Success)
            {
                ModelState.AddModelError("", $"Error: {result.ErrorMessage}");
                return View(request);
            }

            ViewBag.Message = "Login successful!";
            return View();
        }

    }
}
