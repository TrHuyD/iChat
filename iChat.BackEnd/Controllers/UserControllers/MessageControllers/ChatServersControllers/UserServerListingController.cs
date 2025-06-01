using iChat.BackEnd.Services.Users.ChatServers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [Route("api/Chat")]
    [Authorize]
    public class UserServerListingController : ControllerBase
    {
        private readonly ServerListService _userServerListService;

        public UserServerListingController(ServerListService userServerListService)
        {
            _userServerListService = userServerListService;
        }
        [HttpGet]
        public async Task<IActionResult> ServerList()
        {
            var userId = new UserClaimHelper(User).GetUserIdStr();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var servers = await _userServerListService.GetServerList(userId);
            return Ok(servers);
            //  return View("~/Views/User/ChatServer/Listing.cshtml", servers); 
        }
        [HttpGet("{serverID}\\ChannelList")]
        public async Task<List<string>?> ChannelList(string serverID)
        {
            var channels = await _userServerListService.GetChannelList(serverID);
            return channels;
        }
        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> test()
        {

            var userId = "2";
            var servers = await _userServerListService.GetServerList(userId);
            return Ok(servers);
            //         return View("~/Views/User/ChatServer/Listing.cshtml", servers);
        }
    }
}
