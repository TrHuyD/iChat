using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using System;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreMessageWriteService : IMessageDbWriteService
    {
        private readonly MessageWriteQueueService _queueService;

        public EfCoreMessageWriteService(MessageWriteQueueService queueService)
        {
            _queueService = queueService;
        }

        public async Task UploadMessageAsync(MessageRequest request, SnowflakeIdDto messageId)
        {
            if (string.IsNullOrEmpty(request.SenderId))
                throw new ArgumentException("SenderId is required.");

            _queueService.Enqueue(request, messageId);

        }

        public async Task UploadMessagesAsync(IEnumerable<(MessageRequest request, SnowflakeIdDto messageId)> messages)
        {
            foreach (var (req, id) in messages)
            {
                _queueService.Enqueue(req, id);
            }


        }
    }

}
