using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [Route("Chat")]
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
            var request = new UserGetRecentMessageRequest
            {
                ChannelId = channelId,
                UserId = "2"
            };
            var messages = await _chatReadMessageService.RetrieveRecentMessage(request);
            return Ok(messages);
        }
        [HttpGet("{channelId}/read")]
        public async Task<IActionResult> ReadMessages(string channelId)
        {
            var userId = new UserClaimHelper(User).GetUserIdStr();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }
            var request = new UserGetRecentMessageRequest
            {
                ChannelId = channelId,
                UserId = "2"
            };
            var messages = await _chatReadMessageService.RetrieveRecentMessage(request);
            return Ok(messages);
        }
    }
}
