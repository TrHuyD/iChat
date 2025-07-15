using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.UtilServices;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using NRedisStack.DataTypes;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class MessageWriteQueueService : PeriodicService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentQueue<(MessageRequest request, SnowflakeIdDto messageId)> _queue = new();

        MessageTimeLogger _timelogger;
        public MessageWriteQueueService(IServiceScopeFactory scopeFactory, MessageTimeLogger timelogger)
            : base(TimeSpan.FromSeconds(1))
        {
            _scopeFactory = scopeFactory;
            _timelogger = timelogger;
        }

        public void Enqueue(MessageRequest request, SnowflakeIdDto messageId)
        {
            _queue.Enqueue((request, messageId));
        }

        protected override bool EnableRequirement() => !_queue.IsEmpty;

        protected override async Task ExecuteTask()
        {

            var entities = new List<Message>();

            while (_queue.TryDequeue(out var item))
            {
                var entity = new Message
                {
                    Id = item.messageId.Id,
                    ChannelId = long.Parse(item.request.ReceiveChannelId),
                    SenderId = long.Parse(item.request.SenderId),
                    TextContent = item.request.TextContent,
                    Timestamp = item.messageId.CreatedAt
                }
            ;

                if (item.request.MediaFileMetaData == null)
                {
                    entity.MessageType = 1;
                }
                else
                {
                    entity.MessageType = 2;
                    entity.MediaId = item.request.MediaFileMetaData.Id;
                }
                    entities.Add(entity);
            }

            if (entities.Count == 0) return;
            _timelogger.UpdateWriteTime();
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<iChatDbContext>();
            dbContext.Messages.AddRange(entities);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving messages: {ex.Message}");
            }
        }
    }
}
