using Microsoft.AspNetCore.Mvc;
using iChat.DTOs.Users.Auth;
using System.Threading.Tasks;
using iChat.BackEnd.Services.Users.Auth;
using iChat.DTOs.Shared;
using iChat.BackEnd.Services.Users.Auth.Sql;

namespace iChat.BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
  

        public AuthController()
        {
            
        }
        [HttpGet("SessionLogin")]
        public async Task<IActionResult> SessionLogin(
            [FromBody] LoginRequest request,
            [FromServices] ILoginService loginService)
        {
            return Ok((await loginService.LoginShortSession(request)).Value);
        }
        [HttpGet("refreshtoken")]
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
        [HttpPost("refreshtoken/logout")]
        public async Task<IActionResult> LogOut([FromServices] SqlKeyRotationService _service)
        {
            try
            {
                await _service.Logout(HttpContext);
                return Ok();
            }
            catch(Exception ex)
            { return BadRequest(ex.Message); }
        }
        //  [ValidateAntiForgeryToken]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromServices] IRegisterService registerService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(OperationResult.Fail("ValidationError", "Invalid registration data."));
            }

            var result = await registerService.RegisterAsync(request);
            if (!result.Success)
            {
                return Ok(result);
            }

            return Ok(OperationResult.Ok());
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            [FromServices] ILoginService loginService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await loginService.LoginAsync(request, HttpContext);
            if (!result.Success)
            {
                return Unauthorized(new { error = result.ErrorMessage ?? "Invalid username or password" });
            }

            return Ok(new { message = "Login successful!" });
        }

        [HttpGet(".well-known/jwks.json")]
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
