using iChat.BackEnd.Services.Users;
using iChat.BackEnd.Services.Users.Auth;
using iChat.Data.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iChat.BackEnd.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserApiController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null)
                return NotFound();

            return Ok(userProfile); 
        }
    }
}
