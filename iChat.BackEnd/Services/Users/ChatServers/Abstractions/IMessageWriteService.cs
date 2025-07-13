using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IMessageWriteService
    {
        Task<OperationResultT<ChatMessageDtoSafe>> SendTextMessageAsync(MessageRequest request);
        Task<EditMessageRt> EditMessageAsync(UserEditMessageRq rq,string UserId);
        Task<DeleteMessageRt> DeleteMessageAsync(UserDeleteMessageRq rq, string UserId);
    }
}
