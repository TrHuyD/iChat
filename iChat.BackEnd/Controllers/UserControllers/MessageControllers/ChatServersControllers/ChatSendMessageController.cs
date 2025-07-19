using iChat.BackEnd.Models.User;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatSendMessageController :ControllerBase
    {
        readonly IMessageWriteService _writeService;
        readonly ChatHubResponer _chatHub;
        public ChatSendMessageController(IMessageWriteService writeService, ChatHubResponer chatHub)
        {
            _writeService = writeService;
            _chatHub = chatHub;
        }
        [HttpPost("EditMessage")]
        public async Task<IActionResult> EditMessage([FromBody] UserEditMessageRq rq)
        {
            if (rq == null ||!ModelState.IsValid)
            {
                return BadRequest($"Invalid request data.");
            }
            string userId = new UserClaimHelper(User).GetUserIdStr();
            try
            {
                var result = await _writeService.EditMessageAsync(rq, userId);
                if (!result.Success)
                    return BadRequest($"Fail to edit message" + result.ErrorMessage);
                await _chatHub.EditedMessage(result.Value);
            }
            catch(Exception ex)
            {
                return BadRequest($"Fail to edit message {ex.Message}");
            }

            return Ok("Message edited successfully.");
        }
        [HttpPost("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody] UserDeleteMessageRq rq)
        {
            if (rq == null || !ModelState.IsValid)
            {
                return BadRequest($"Invalid request data.");
            }
            string userId = new UserClaimHelper(User).GetUserIdStr();
            try
            {
                var result = await _writeService.DeleteMessageAsync(rq, userId);
                if (!result.Success)
                    return BadRequest($"Fail to delete message" + result.ErrorMessage);
               await _chatHub.DeletedMessage(result.Value);
            }
            catch (Exception ex)
            {   
                return BadRequest($"Fail to delete message {ex.Message}");
            }
                return Ok("Message deleted successfully.");
        }
        [HttpPost("UploadMessage")]
        [RequestSizeLimit(3145728)]
        public async Task<IActionResult> SendMessage( [FromForm] MessageUploadRequest request)
        {
            var userId = new UserClaimHelper(User).GetUserIdStr();
            var result = await _writeService.SendMediaMessageAsync(request, userId,true);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            await _chatHub.NewMessage(result.Value, request.ServerId);
            return Ok();
        }
        //[HttpPost]
        //[HttpPost("{channelId}/send")]
        //public async Task<IActionResult> SendMessageApi()
    }
}
