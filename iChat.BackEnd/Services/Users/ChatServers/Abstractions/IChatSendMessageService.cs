using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatSendMessageService
    {
        Task<OperationResultT<ChatMessageDto>> SendTextMessageAsync(MessageRequest request);
    }
}
