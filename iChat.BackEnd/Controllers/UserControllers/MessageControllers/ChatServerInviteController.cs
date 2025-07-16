using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
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

        IChatServerMetadataCacheService _localcache;
        public ChatServerInviteController(RedisCSInviteLinkService service,IChatServerMetadataCacheService localcache)
        {
            _localcache = localcache;
            _service = service;
        }
        [HttpGet("{serverId}/InviteLink")]
        public async Task<IActionResult> GetInviteLink(string serverId)
        {
            var userId = new UserClaimHelper(User).GetUserIdStr();
            try
            {
                var link = await _service.CreateInviteLink(serverId, userId);
                return Ok(link);
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
                return Ok(await _localcache.GetServerAsync(serverId,false));
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
        public async Task<IActionResult> UseInviteLink(string inviteId, [FromServices] AppChatServerService _joinService, [FromServices] ChatHubResponer hub, [FromServices] IChatServerMetadataCacheService cacheService)
        {
            var userId = new UserClaimHelper(User).GetUserIdStr();
            var uId= long.Parse(userId);    
            try
            {
                var serverIdstr = await _service.ParseInviteLink(inviteId);
                var serverId = long.Parse(serverIdstr);
                await _joinService.Join(uId, serverId);
                await hub.JoinNewServer(userId, await cacheService.GetServerAsync(serverIdstr));
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
