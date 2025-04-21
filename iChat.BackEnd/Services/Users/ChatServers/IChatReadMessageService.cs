using iChat.BackEnd.Models.User.MessageRequests;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public interface IChatReadMessageService
    {

        public Task<List<ChatMessageDto>> RetrieveRecentMessage(UserGetRecentMessageRequest request);
    }
}
