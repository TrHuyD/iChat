using iChat.BackEnd.Services.Users;
using iChat.BackEnd.Services.Users.Auth;
using iChat.BackEnd.Services.Users.ChatServers;
using iChat.Data.Entities.Users;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iChat.BackEnd.Controllers
{
    [Route("api/users")]
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
            var userId = new UserClaimHelper(User).GetUserId();

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null)
                return NotFound();

            return Ok(userProfile);
        }
        [HttpGet("CompleteInfo")]
        [Authorize]
        public async Task<IActionResult> GetCompleteInfo([FromServices]ServerListService serverListService )
        {
            var userId = new UserClaimHelper(User).GetUserId();
            var userProfile = await _userService.GetUserProfileAsync(userId);
            var userServerList = await serverListService.GetServerList(userId);
            List<ChatServerDto> chatServerDtos = new List<ChatServerDto>();
            foreach( var i in userServerList)
            {
                chatServerDtos.Add(new ChatServerDto { Id = i,Name=i });
            }
            var package = new UserCompleteDto
            {
                UserProfile = userProfile,
                ChatServers = chatServerDtos
            };
            if (package == null)
                return NotFound();
            return Ok(package);
        }
    }

}