using iChat.BackEnd.Models.Helpers;

namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class MessageReadService : CasandraService
    {
        public MessageReadService(CassandraOptions options) : base(options)
        {
            var query = "SELECT id, userId, content, timestamp FROM messages WHERE channelId = ? LIMIT 25";
        }
    }
}
