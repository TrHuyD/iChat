using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
using iChat.DTOs.ChatServerDatas;
using iChat.DTOs.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    [ApiController]
    [Route("api/Chat")]
    [Authorize]
    public class ChatServerInviteController :ControllerBase
    {
        private readonly RedisCSInviteLinkService _service;

        IChatServerRepository _localcache;
        public ChatServerInviteController(RedisCSInviteLinkService service, IChatServerRepository localcache)
        {
            _localcache = localcache;
            _service = service;
        }
        [HttpGet("{serverId}/InviteLink")]
        public async Task<IActionResult> GetInviteLink(string serverId)
        {
            var userId = new UserClaimHelper(User).GetUserIdSL();
            var _serverId = new ServerId(new stringlong(serverId));
            try
            {
                var link = await _service.CreateInviteLink(_serverId, userId);
                //if(link)
                return Ok(link.Value);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }
        [HttpGet("InviteLink/{inviteId}")]
        public async Task<IActionResult> ParseInviteLink(string inviteId )
        {
            try
            {
                var serverId = await _service.ParseInviteLink(inviteId);
                var result = _localcache.GetServerAsync(serverId, false);
                if (result.Failure)
                    return BadRequest("Internal error " + result.ErrorMessage);
                return Ok(result.Value.toMetadata() );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpPost("InviteLink/{inviteId}")]
        public async Task<IActionResult> UseInviteLink(string inviteId, [FromServices] AppChatServerService _joinService, [FromServices] ChatHubResponer hub)
        {
            var userId = new UserClaimHelper(User).GetUserIdSL();
            try
            {
                var serverId = await _service.ParseInviteLink(inviteId);

                await _joinService.Join(userId, serverId);
                //var result = _localcache.GetServerAsync(serverId, false);
                //if (!result.Success)
                //    throw new Exception("Server is not loaded");
                await hub.JoinNewServer(userId, serverId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
