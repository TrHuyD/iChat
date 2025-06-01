using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatSendMessageController :ControllerBase
    {
        readonly IChatSendMessageService _writeService;
        public ChatSendMessageController(IChatSendMessageService writeService)
        {
            _writeService = writeService;
        }
        [HttpPost("{channelId}/send")]
        public async Task<IActionResult> SendMessage(string channelId, [FromBody] UserWeb_MessageRequest request)
        {
            if (!long.TryParse(channelId, out long longChannelId))
            {
                return BadRequest("Valid channel ID is required.");
            }
            if (request == null)
            {
                return BadRequest("Message request is required.");
            }
            string userId = new UserClaimHelper(User).GetUserIdStr();
            var r = new MessageRequest
            {
                SenderId= userId,
                ReceiveChannelId = channelId,
                TextContent = request.TextContent,
                MediaContent = request.MediaContent,
            };
            var result = await _writeService.SendTextMessageAsync(r);
            if (result.Success)
            {
                return Ok("Message sent successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to send message.");
            }
        }
        //[HttpPost]
        //[HttpPost("{channelId}/send")]
        //public async Task<IActionResult> SendMessageApi()
    }
}
