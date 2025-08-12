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

                var userId = new UserClaimHelper(User).GetUserIdSL();
                var result= await createService.CreateServerAsync(rq, userId);
                if (!result.Success)
                    return BadRequest(result.ErrorMessage);
                await responer.JoinNewServer(userId, result.Value.Id);
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
        //    ViewData["Id"] = id;
        //    return PartialView("_CreateChannel"); 
        //}

        [HttpPost("CreateChannel")]
        public async Task<IActionResult> CreateChannel([FromBody] ChatChannelCreateRq rq)
        {

            var userId = new UserClaimHelper(User).GetUserIdSL();
            try
            {
                var result = await createService.CreateChannelAsync(rq,userId);
                if(!result.Success)
                    return BadRequest(result.ErrorMessage);
                var channel = result.Value;
                await responer.NewChannel(channel.ServerId.ToString(), channel);
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
