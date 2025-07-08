using iChat.BackEnd.Services.Users.ChatServers.Abstractions;

using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    [Route("api/Chat")]
    [Authorize]
    [ApiController]
    public class ChatServerController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatCreateService _service;
        public ChatServerController(IChatCreateService service, IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
            _service = service;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateServer([FromBody] ChatServerCreateRq rq)
        {
            var name = rq.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Server name is required.");
                return BadRequest(ModelState);
            }

            var userId = new UserClaimHelper(User).GetUserId();
            var serverid = await _service.CreateServerAsync(name, userId);
            return Ok(serverid);
        }
        //[HttpGet("{id}/CreateChannel")]
        //public IActionResult CreateChannel(long id)
        //{
        //    ViewData["ServerId"] = id;
        //    return PartialView("_CreateChannel"); 
        //}

        [HttpPost("CreateChannel")]
        public async Task<IActionResult> CreateChannel([FromBody] ChatChannelCreateRq rq)
        {

            var userId = new UserClaimHelper(User).GetUserId();
            try
            {
                var channel = await _service.CreateChannelAsync(long.Parse(rq.ServerId), rq.Name, userId);
                await _hubContext.Clients.Groups(rq.ServerId).SendAsync("ChannelCreate", channel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }
        [HttpGet("{serverId:long}/GetLastSeenlist")]
        public async Task<IActionResult> GetLastSeenList(string serverId, [FromServices] IMessageLastSeenService _service)
        {
            var userId = new UserClaimHelper(User).GetUserIdStr();
            var lastSeenList = await _service.GetLastSeenMessageAsync(serverId, userId);
            return Ok(lastSeenList);
        }
    }
}
