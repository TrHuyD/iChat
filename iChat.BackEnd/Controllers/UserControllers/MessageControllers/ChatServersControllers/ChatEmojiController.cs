using iChat.BackEnd.Models.ChatServer;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.BackEnd.Services.Users.ChatServers.Application.ChatServer;
using iChat.DTOs.Collections;
using iChat.DTOs.Users.Servers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [ApiController]
    [Authorize]
    [Route("api/chat")]
    public class ChatEmojiController :ControllerBase
    {
        public ChatEmojiController() { }
        [HttpPost("{serverId}/emojis")]
        public async Task<IActionResult> AddEmoji([FromRoute] string serverId, [FromForm] AddEmojiRequest request, [FromServices] AppEmojiWriteService writeService )
        {
            var userID = new UserClaimHelper(User).GetUserIdSL();
            var crequest = new CompleteEmojiRequest
            {
                File = request.file,
                UserId = userID,
                ServerId =new ServerId( serverId)
            };
            var result = await writeService.Create(crequest);
            if (result.Failure)
                return BadRequest(result.ErrorMessage);
            return Ok();  
        }
        [HttpGet("{serverId}/emojis")]
        public async Task<IActionResult> GetEmoji([FromRoute] string serverId, [FromServices]IEmojiRepository emojiCacheService)
        {
            return Ok(emojiCacheService.GetEmojisAsync(new ServerId(serverId)).Value);
        }
        
    }
}
