using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
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
        readonly IMessageWriteService _chatWriteService;
        readonly RedisChatCache _redis_dbservice;
        readonly SnowflakeService _idGen;
        public Test_UserSendTextMessageService(IMessageWriteService dbservice
            ,RedisChatCache rWService
            ,SnowflakeService snowflakeService)
        {
            _chatWriteService = dbservice;
            _redis_dbservice = rWService;
            _idGen = snowflakeService;
        }

        public async Task<OperationResultT<ChatMessageDto>> SendTextMessageAsync(MessageRequest request)
        {
            var messageIdResult = _idGen.GenerateId();

            // Faf Cassandra upload
            _ = Task.Run(() => _chatWriteService.UploadMessageAsync(request, messageIdResult));

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
