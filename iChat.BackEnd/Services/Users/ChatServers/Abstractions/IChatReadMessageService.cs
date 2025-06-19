using iChat.BackEnd.Models.User.MessageRequests;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatReadMessageService
    {
        Task<List<ChatMessageDto>> RetrieveRecentMessage(UserGetRecentMessageRequest request);
        Task<List<ChatMessageDto>> GetMessagesContainingAsync(long channelId, long messageId);
    }
}
