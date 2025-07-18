using iChat.BackEnd.Models.User;
using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IMessageWriteService
    {
        Task<OperationResultT<NewMessage>> SendTextMessageAsync(MessageRequest request,string ServerId);
        Task<OperationResultT<EditMessageRt>> EditMessageAsync(UserEditMessageRq rq,string UserId);
        Task<OperationResultT<DeleteMessageRt>> DeleteMessageAsync(UserDeleteMessageRq rq, string UserId);
        Task<OperationResultT<NewMessage>> SendMediaMessageAsync(MessageUploadRequest rq, string UserId);
    }
}
