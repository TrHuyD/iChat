using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.CassandraDB;
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

        public Test_UserSendTextMessageService( CassandraMessageWriteService dbservice,RedisMessageRWService rWService )
        {
            _cas_dbservice = dbservice;
            _redis_dbservice = rWService;
        }

        public async Task<OperationResultT<ChatMessageDto>> SendTextMessageAsync(MessageRequest request)
        {
            var cas_result= await _cas_dbservice.UploadMessageAsync(request);
            if(!cas_result.Success)
                return OperationResultT<ChatMessageDto>.Fail("400","");
            
            var rt = new ChatMessageDto
            {
                Id = cas_result.MessageId,
                Content = request.TextContent,
                ContentMedia = request.MediaContent,
                MessageType = (int)MessageType.Text,
                SenderId = long.Parse(request.SenderId),
                RoomId = long.Parse(request.ReceiveChannelId),
                CreatedAt=cas_result.CreatedAt
            };
            _ = Task.Run(() => _redis_dbservice.UploadMessageAsync(cas_result.MessageId, rt));
            return OperationResultT<ChatMessageDto>.Ok(rt);
        }
    }

}
