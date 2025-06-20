using iChat.BackEnd.Models.Helpers;
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
            var userId = new UserClaimHelper(User).GetUserId();


            var servers = await _userServerListService.get(userId);
            return Ok(servers);
            //  return View("~/Views/User/ChatServer/Listing.cshtml", servers); 
        }
        [HttpGet("{serverID}\\ChannelList")]
        public async Task<IActionResult> ChannelList(string serverID)
        {
            if(!ValueParser.TryLong(serverID, out var _serverId))
                return BadRequest("Invalid server ID format.");
            var channels = await _userServerListService.GetChannelList(_serverId);
            return Ok(channels);
        }

    }
}
