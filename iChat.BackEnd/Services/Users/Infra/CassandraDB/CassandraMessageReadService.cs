using iChat.BackEnd.Models.Helpers.CassandraOptionss;
using iChat.DTOs.Users.Messages;
using ISession = Cassandra.ISession;
namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class CassandraMessageReadService 
    {
        private Lazy<CasandraService> service;
        public CassandraMessageReadService(Lazy<CasandraService> _cs)
        {
            service = _cs;
        }
        public async Task<List<ChatMessageDto>> GetMessagesByChannelAsync(string channelId, int limit = 40)
        {

            var query = $@"
                            SELECT message_id, sender_id, message_type, text_content, media_content, timestamp
                            FROM user_upload.messages
                            WHERE channel_id = ?
                            LIMIT {limit};";

            var session = service.Value.GetSession();
            var preparedStatement = await session.PrepareAsync(query);
            var boundStatement = preparedStatement.Bind(long.Parse(channelId));
            var resultSet = await session.ExecuteAsync(boundStatement);
            var messages = new List<ChatMessageDto>();
            foreach (var row in resultSet)
            {
                messages.Add(new ChatMessageDto
                {
                    Id = row.GetValue<long>("message_id"),
                    SenderId = row.GetValue<long>("sender_id"),
                    MessageType = row.GetValue<short>("message_type"),
                    Content = row.GetValue<string>("text_content") ?? string.Empty,
                    ContentMedia = row.GetValue<string>("media_content") ?? string.Empty,
                    CreatedAt = row.GetValue<DateTime>("timestamp")
                });
            }

            return messages;
        }
    }
}
