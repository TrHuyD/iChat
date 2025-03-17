using iChat.BackEnd.Services.Users.Auth;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers
{
    [Route("register")]
    public class UserRegisterController : Controller
    {
        private readonly CreateUserService _UserService;
        public UserRegisterController(CreateUserService UserService)
        {
            _UserService = UserService;
        }
        [HttpGet()]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View("~/Views/User/Register.cshtml");
        }
        [HttpPost()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View("~/Views/User/Register.cshtml", request);

            try
            {
                await _UserService.RegisterAsync(request);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(request);
            }
        }
    }

}