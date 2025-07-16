using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
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
        private readonly ChatHubResponer responer;
        private readonly AppChatServerCreateService createService;
        public ChatServerController( ChatHubResponer responer, AppChatServerCreateService createService)
        {
            this.responer = responer;
            this.createService= createService;
           
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateServer([FromBody] ChatServerCreateRq rq)
        {
            try {
                if (string.IsNullOrWhiteSpace(rq.Name))
                {
                    ModelState.AddModelError("Name", "Server name is required.");
                    return BadRequest(ModelState);
                }

                var userId = new UserClaimHelper(User).GetUserId();
                var serverid = await createService.CreateServerAsync(rq, userId);
                await responer.JoinNewServer(userId.ToString(), serverid);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                var channel = await createService.CreateChannelAsync(rq,userId);
                await responer.NewChannel(channel.ServerId, channel);
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
