using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public interface IChatSendMessageService
    {
        Task<bool> SendTextMessageAsync(MessageRequest request);
    }
}
