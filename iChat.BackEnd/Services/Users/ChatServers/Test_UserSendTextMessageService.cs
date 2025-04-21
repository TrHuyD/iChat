using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.BackEnd.Services.Validators.TextMessageValidators;

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

        public async Task<bool> SendTextMessageAsync(MessageRequest request)
        {
            var cas_result= await _cas_dbservice.UploadMessageAsync(request);
            if(!cas_result.Success)
                return false;
            _ = Task.Run(() => _redis_dbservice.UploadMessageAsync(cas_result.MessageId,request));
            return cas_result.Success;
        }
    }

}
