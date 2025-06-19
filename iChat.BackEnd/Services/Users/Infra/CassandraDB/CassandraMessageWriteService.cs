using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using Cassandra;
using ISession = Cassandra.ISession;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;

namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class CassandraMessageWriteService : IMessageWriteService
    {
        private readonly ISession _session;

        public CassandraMessageWriteService(CasandraService cassandraService)
        {
            _session = cassandraService.GetSession();
        }

        public async Task<DbWriteResult> UploadMessageAsync(MessageRequest request, SnowflakeIdDto messageId)
        {
            if (string.IsNullOrEmpty(request.SenderId))
                throw new ArgumentException("SenderId is required.");

            var offsetTimestamp = messageId.CreatedAt;
            var timestamp = offsetTimestamp.UtcDateTime;

            const string query = @"
                INSERT INTO user_upload.messages 
                (channel_id, message_id, sender_id, message_type, text_content, media_content, timestamp) 
                VALUES (?, ?, ?, ?, ?, ?, ?);";

            var prepared = await _session.PrepareAsync(query);
            var bound = prepared.Bind(
                long.Parse(request.ReceiveChannelId),
                messageId.Id,
                long.Parse(request.SenderId),
                (short)request.messageType,
                request.TextContent ?? string.Empty,
                request.MediaContent ?? string.Empty,
                timestamp
            );

            await _session.ExecuteAsync(bound);

            return new DbWriteResult
            {
                Success = true,
                CreatedAt = offsetTimestamp
            };
        }

        public Task<DbWriteResult> UploadMessagesAsync(IEnumerable<(MessageRequest request, SnowflakeIdDto messageId)> messages)
        {
            throw new NotImplementedException();
        }
    }
}
