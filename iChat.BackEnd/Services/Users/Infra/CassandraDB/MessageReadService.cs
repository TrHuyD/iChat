using iChat.BackEnd.Models.Helpers.CassandraOptionss;
using iChat.DTOs.Users.Messages;
using ISession = Cassandra.ISession;
namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class MessageReadService 
    {
        private ISession session;
        public MessageReadService(CasandraService _cs)
        {
           session= _cs.GetSession();
        }
        public async Task<List<ChatMessageDto>> GetMessagesByChannelAsync(long channelId, int limit = 50)
        {
            var query = "SELECT message_id, sender_id, message_type, text_content, media_content, timestamp " +
                        "FROM db_user_message.messages " +
                        "WHERE channel_id = ? " +
                        "LIMIT ?;";

            var preparedStatement = await session.PrepareAsync(query);
            var boundStatement = preparedStatement.Bind(channelId, limit);

            var resultSet = await session.ExecuteAsync(boundStatement);
            var messages = new List<ChatMessageDto>();

            foreach (var row in resultSet)
            {
                messages.Add(new ChatMessageDto
                {
                    Id = row.GetValue<long>("message_id"),
                    SenderId = row.GetValue<long>("sender_id"),
                    MessageType = row.GetValue<int>("message_type"),
                    Content = row.GetValue<string>("text_content") ?? string.Empty,
                    ContentMedia = row.GetValue<string>("media_content") ?? string.Empty,
                    CreatedAt = row.GetValue<DateTime>("timestamp"),
                    ChannelId = channelId
                });
            }

            return messages;
        }
    }
}
