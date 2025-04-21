using iChat.BackEnd.Services.Users.ChatServers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [Route("Chat")]
    [Authorize]
    public class UserServerListingController : Controller
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
            return View("~/Views/User/ChatServer/Listing.cshtml", servers); 
        }
        [HttpGet("{serverID}\\api\\ChannelList")]
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
            return View("~/Views/User/ChatServer/Listing.cshtml", servers);
        }
    }
}
