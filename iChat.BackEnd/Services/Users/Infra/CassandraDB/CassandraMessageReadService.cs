using Cassandra;
using iChat.BackEnd.Models.Helpers.CassandraOptionss;
using iChat.DTOs.Users.Messages;
using NRedisStack.DataTypes;
using ISession = Cassandra.ISession;
namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class CassandraMessageReadService
    {
        private readonly Lazy<CasandraService> service;

        public CassandraMessageReadService(Lazy<CasandraService> _cs)
        {
            service = _cs;
        }

        public async Task<List<ChatMessageDto>> GetMessagesByChannelAsync(string channelId, int limit = 40)
        {
            var query = @"
            SELECT message_id, sender_id, message_type, text_content, media_content, timestamp
            FROM user_upload.messages
            WHERE channel_id = ?
            ORDER BY message_id DESC
            LIMIT ?;";

            var session = service.Value.GetSession();
            var prepared = await session.PrepareAsync(query);
            var bound = prepared.Bind(long.Parse(channelId), limit);
            var result = await session.ExecuteAsync(bound);

            return MapResultSet(result);
        }
      
        public async Task<List<ChatMessageDto>> GetMessagesAroundMessageIdAsync(string channelId, long messageId, int before = 20, int after = 22)
        {
            var session = service.Value.GetSession();

            string queryBefore = @"
        SELECT message_id, sender_id, message_type, text_content, media_content, timestamp
        FROM user_upload.messages
        WHERE channel_id = ?
        AND message_id <= ?
        ORDER BY message_id DESC
        LIMIT ?;";

            string queryAfter = @"
        SELECT message_id, sender_id, message_type, text_content, media_content, timestamp
        FROM user_upload.messages
        WHERE channel_id = ?
        AND message_id > ?
        ORDER BY message_id ASC
        LIMIT ?;";

            // Prepare both queries
            var stmtBefore = await session.PrepareAsync(queryBefore);
            var stmtAfter = await session.PrepareAsync(queryAfter);

            var boundBefore = stmtBefore.Bind(long.Parse(channelId), messageId, before + 1);
            var boundAfter = stmtAfter.Bind(long.Parse(channelId), messageId, after);

            // Execute both queries concurrently
            var beforeTask = session.ExecuteAsync(boundBefore);
            var afterTask = session.ExecuteAsync(boundAfter);
            await Task.WhenAll(beforeTask, afterTask);

            var beforeMessages = ParseToDto(beforeTask.Result);

            var afterMessages = ParseToDto(afterTask.Result);

            beforeMessages.Reverse(); 

            return beforeMessages.Concat(afterMessages).ToList();
        }
        private List<ChatMessageDto> ParseToDto(RowSet resultSet)
        {
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
                    CreatedAt = new DateTimeOffset(row.GetValue<DateTime>("timestamp"), TimeSpan.Zero)
                });
            }
            return messages;
        }
        public async Task<List<ChatMessageDto>> GetMessagesInRangeAsync(string channelId, long startId, long endId, int limit = 50)
        {
            var query = @"
            SELECT message_id, sender_id, message_type, text_content, media_content, timestamp
            FROM user_upload.messages
            WHERE channel_id = ?
            AND message_id >= ?
            AND message_id <= ?
            ORDER BY message_id ASC
            LIMIT ?;";

            var session = service.Value.GetSession();
            var prepared = await session.PrepareAsync(query);
            var bound = prepared.Bind(long.Parse(channelId), startId, endId, limit);
            var result = await session.ExecuteAsync(bound);

            return MapResultSet(result);
        }

        private List<ChatMessageDto> MapResultSet(RowSet resultSet)
        {
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
                    CreatedAt = new DateTimeOffset(row.GetValue<DateTime>("timestamp"), TimeSpan.Zero)
                });
            }
            return messages;
        }
    }

}
