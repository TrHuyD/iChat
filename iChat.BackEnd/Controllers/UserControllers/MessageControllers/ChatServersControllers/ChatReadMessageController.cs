using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [Route("api/Chat")]
    [ApiController]
    [Authorize]
    public class ChatReadMessageController : ControllerBase
    {
        private readonly IChatReadMessageService _chatReadMessageService;
        public ChatReadMessageController(IChatReadMessageService chatReadMessageService)
        {
            _chatReadMessageService = chatReadMessageService;
        }
        [HttpGet("{channelId}/readtest")]
        [AllowAnonymous]
        public async Task<IActionResult> ReadMessages_test(string channelId)
        {
            //var userId = new UserClaimHelper(User).GetUserIdStr();
            //if (string.IsNullOrEmpty(userId))
            //{
            //    return BadRequest("User ID is required.");
            //}
            if(long.TryParse(channelId, out long channelIdLong) == false|| (channelIdLong <= 1000000000000000l))
            {
                return BadRequest("Invalid channel ID format.");
            }

            var request = new UserGetRecentMessageRequest
            {
                ChannelId = channelIdLong,
                UserId = 2
            };
            var messages = await _chatReadMessageService.RetrieveRecentMessage(request);
            return Ok(messages);
        }
        [HttpGet("{channelId}/read")]
        public async Task<IActionResult> ReadMessages(string channelId)
        {
            var userId = new UserClaimHelper(User).GetUserId();
            if (long.TryParse(channelId, out long channelIdLong) == false || (channelIdLong <= 1000000000000000l))
            {
                return BadRequest("Invalid channel ID format.");
            }
            var request = new UserGetRecentMessageRequest
            {
                ChannelId = channelIdLong,
                UserId = userId
            };
            var messages = await _chatReadMessageService.RetrieveRecentMessage(request);
            return Ok(messages);
        }
        [HttpGet("{channelId}/history")]
        public async Task<IActionResult> GetMessageHistory(string channelId,[FromQuery] string? beforeMessageId = null)
        {
            if (long.TryParse(channelId, out long channelIdLong) == false || (channelIdLong <= 1000000000000000l))
            {
                return BadRequest("Invalid channel ID format.");
            }
            
            if (!string.IsNullOrEmpty(beforeMessageId) && long.TryParse(beforeMessageId, out var beforeMessageIdLong))
            {
                
            }
            else
                return BadRequest("Invalid Message ID format.");
            var messages = await _chatReadMessageService.GetMessagesBeforeAsync(channelIdLong, beforeMessageIdLong);
            return Ok(messages);
        }
    }
}
