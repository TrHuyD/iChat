using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;

namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public interface IMessageDbWriteService
    {
        Task UploadMessageAsync(MessageRequest request, SnowflakeIdDto messageId);
        Task UploadMessagesAsync(IEnumerable<(MessageRequest request, SnowflakeIdDto messageId)> messages);

    }
}
