using Microsoft.AspNetCore.Mvc;
using iChat.DTOs.Users.Auth;
using iChat.BackEnd.Services.Users.Auth.Auth0;
using System.Threading.Tasks;
using iChat.BackEnd.Services.Users.Auth;
using iChat.DTOs.Shared;
using iChat.BackEnd.Services.Users.Auth.Sql;

namespace iChat.BackEnd.Controllers
{
    [Route("Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
  

        public AuthController()
        {
            
        }
        [HttpGet(UrlPath.RefreshToken)]
        public async Task<IActionResult> RefreshToken([FromServices] SqlKeyRotationService _service)
        {
            var result = await _service.RefreshCred(HttpContext);
            if (!result.Success)
            {
                ModelState.AddModelError("", $"Error: {result.ErrorMessage}");
                return BadRequest(ModelState);
            }
            var token = result.Value;
            return Ok(token);
        }
        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody]RegisterRequest request,[FromServices] IRegisterService _registerService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _registerService.RegisterAsync(request);
            if (!result.Success)
            {
                ModelState.AddModelError("", $"Error: {result.ErrorMessage}");
                return BadRequest(ModelState);
            }
            return Ok(new { message = "User created successfully!" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] ILoginService loginService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await loginService.LoginAsync(request, HttpContext);
            if (!result.Success)
            {
                ModelState.AddModelError("", $"Error: {result.ErrorMessage}");
                return BadRequest(ModelState);
            }
            return Ok(new { message = "Login successful!" });
        }
        [HttpGet("/.well-known/jwks.json")]
        public IActionResult GetJwks([FromServices] JwtService service)
        {
            var jwks = service.GetPublicJwk();
            return Ok(jwks);
        }
        //[HttpGet("register")]
        //public IActionResult Register()
        //{
        //    return View("Register");
        //}

        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterRequest request)
        //{
        //    if (!ModelState.IsValid)
        //        return View(request);

        //    var result = await _registerService.RegisterAsync(request);

        //    if (!result.Success)
        //    {
        //        ModelState.AddModelError("", $"Error: {result.ErrorMessage}");
        //        return View(request);
        //    }

        //    ViewBag.Message = "User created successfully!";
        //    return View();
        //}
        //[HttpGet("login")]
        //public IActionResult Login()
        //{
        //    return View("Login");
        //}

        //[HttpPost("login")]
        //public async Task<IActionResult> Login(LoginRequest request, [FromServices] ILoginService loginService)
        //{
        //    if (!ModelState.IsValid)
        //        return View(request);

        //    var result = await loginService.LoginAsync(request,HttpContext);

        //    if (!result.Success)
        //    {
        //        ModelState.AddModelError("", $"Error: {result.ErrorMessage}");
        //        return View(request);
        //    }

        //    ViewBag.Message = "Login successful!";
        //    return View();
        //}

    }
}
