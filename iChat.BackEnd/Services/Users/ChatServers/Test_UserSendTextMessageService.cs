using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.BackEnd.Services.Validators.TextMessageValidators;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public class Test_UserSendTextMessageService  : IChatSendMessageService

    {
        readonly CassandraMessageWriteService _cas_dbservice;
        readonly RedisMessageRWService _redis_dbservice;
        readonly SnowflakeService _idGen;
        public Test_UserSendTextMessageService( CassandraMessageWriteService dbservice,RedisMessageRWService rWService, SnowflakeService snowflakeService)
        {
            _cas_dbservice = dbservice;
            _redis_dbservice = rWService;
            _idGen = snowflakeService;
        }

        public async Task<OperationResultT<ChatMessageDto>> SendTextMessageAsync(MessageRequest request)
        {
            var messageIdResult = _idGen.GenerateId();

            // Faf Cassandra upload
            _ = Task.Run(() => _cas_dbservice.UploadMessageAsync(request, messageIdResult.Id));

            var rt = new ChatMessageDto
            {
                Id = messageIdResult.Id,
                Content = request.TextContent,
                ContentMedia = request.MediaContent,
                MessageType = (int)MessageType.Text,
                SenderId = long.Parse(request.SenderId),
                RoomId = long.Parse(request.ReceiveChannelId),
                CreatedAt = messageIdResult.CreatedAt
            };
    
            await _redis_dbservice.UploadMessageAsync(rt);

            return OperationResultT<ChatMessageDto>.Ok(rt);
        }

    }

}
