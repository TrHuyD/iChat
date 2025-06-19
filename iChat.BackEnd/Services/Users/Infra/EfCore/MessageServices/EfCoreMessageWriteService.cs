using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using System;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreMessageWriteService : IMessageWriteService
    {
        private readonly iChatDbContext _dbContext;

        public EfCoreMessageWriteService(iChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DbWriteResult> UploadMessageAsync(MessageRequest request, SnowflakeIdDto messageId)
        {
            if (string.IsNullOrEmpty(request.SenderId))
                throw new ArgumentException("SenderId is required.");

            var message = new Message
            {
                Id = messageId.Id,
                ChannelId = long.Parse(request.ReceiveChannelId),
                SenderId = long.Parse(request.SenderId),
                MessageType = (short)request.messageType,
                TextContent = request.TextContent,
                MediaContent = request.MediaContent,
                Timestamp = messageId.CreatedAt
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return new DbWriteResult
            {
                Success = true,
                CreatedAt = messageId.CreatedAt
            };
        }

        public async Task<DbWriteResult> UploadMessagesAsync(IEnumerable<(MessageRequest request, SnowflakeIdDto messageId)> messages)
        {
            var entityList = new List<Message>();

            foreach (var (request, messageId) in messages)
            {
                if (string.IsNullOrEmpty(request.SenderId))
                    throw new ArgumentException("SenderId is required in batch.");

                var entity = new Message
                {
                    Id = messageId.Id,
                    ChannelId = long.Parse(request.ReceiveChannelId),
                    SenderId = long.Parse(request.SenderId),
                    MessageType = (short)request.messageType,
                    TextContent = request.TextContent,
                    MediaContent = request.MediaContent,
                    Timestamp = messageId.CreatedAt
                };

                entityList.Add(entity);
            }

            _dbContext.Messages.AddRange(entityList);
            await _dbContext.SaveChangesAsync();

            return new DbWriteResult
            {
                Success = true,
                CreatedAt = entityList.Max(m => m.Timestamp) 
            };
        }
    }
}
