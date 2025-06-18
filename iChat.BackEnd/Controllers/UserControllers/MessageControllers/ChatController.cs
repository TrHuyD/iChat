using iChat.BackEnd.Services.Users.ChatServers;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    public class ChatController : ControllerBase
    {

        public async Task<List<ChatMessageDto>> GetMessageHistory([FromServices] IChatReadMessageService _writeService
            ,[FromQuery] long roomId
            ,[FromQuery] long? beforeMessageId = null,
            [FromQuery]long? latestEditTimestamp=null)
        {
            //var rq = new UserGetRecentMessageRequest
            //{
            //    UserId = new UserClaimHelper(Context.User).GetUserIdStr(),
            //    ChannelId = roomId.ToString(),
            //};
            //var message = await _writeService.RetrieveRecentMessage(rq);
            //_logger.LogInformation($"Requesting message history for room {roomId} before message ID {beforeMessageId}");
            return new();
        }


    }
}
