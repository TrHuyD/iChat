using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    public class ChatServerInviteController :ControllerBase
    {
        private readonly RedisCSInviteLinkService _service;
        public ChatServerInviteController(RedisCSInviteLinkService service)
        {
            _service = service;
        }
        [HttpGet("{serverId:long}/InviteLink")]
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
        public async Task<IActionResult> ParseInviteLink(string inviteId, [FromServices] IChatServerMetadataCacheService _localcache)
        {
            try
            {
                var serverId = await _service.ParseInviteLink(inviteId);
                return Ok(_localcache.GetServerAsync(serverId,false));
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
