using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;

using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.BackEnd.Services.Validators.TextMessageValidators;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public class Test_UserSendTextMessageService  : IMessageWriteService

    {
        readonly IMessageDbWriteService _chatWriteService;
       // readonly RedisChatCache _redis_dbservice;
        readonly SnowflakeService _idGen;
        IMessageCacheService _cache;
        readonly IChatServerMetadataCacheService _serverMetaDataCacheService;
        public Test_UserSendTextMessageService(IMessageDbWriteService dbservice,
            IMessageCacheService cache
        //    ,RedisChatCache rWService
            ,SnowflakeService snowflakeService,
            IChatServerMetadataCacheService ServerMetaDataCache)
        {
             _serverMetaDataCacheService = ServerMetaDataCache;
            _cache = cache;
            _chatWriteService = dbservice;
       //     _redis_dbservice = rWService;
            _idGen = snowflakeService;
        }

        public async Task DeleteMessageAsync(UserDeleteMessageRq rq,string UserId)
        {
            var longrq= new DeleteMessageRq
            {
                ChannelId =long.Parse( rq.ChannelId),
                MessageId =long.Parse( rq.MessageId),
                UserId =long.Parse(UserId),
                ServerId=long.Parse(rq.ServerId)
            };
            var isAdmin = await  _serverMetaDataCacheService.IsAdmin(longrq.ServerId,longrq.ChannelId,longrq.UserId);
            var cacheResult =await _cache.DeleteMessageAsync(longrq, isAdmin);
            await _chatWriteService.DeleteMessageAsync(longrq, isAdmin);
            
        }

        public async Task EditMessageAsync(UserEditMessageRq rq, string UserId)
        {
            var longrq= new EditMessageRq
            {
                ChannelId = long.Parse(rq.ChannelId),
                MessageId = long.Parse(rq.MessageId),
                UserId = long.Parse(UserId),
                NewContent = rq.NewContent
            };
            var cacheResult= await _cache.EditMessageAsync(longrq);
            await _chatWriteService.EditMessageAsync(longrq);
        }

        public async Task<OperationResultT<ChatMessageDtoSafe>> SendTextMessageAsync(MessageRequest request)
        {
            var messageIdResult = _idGen.GenerateId();
            _ = Task.Run(() => _chatWriteService.UploadMessageAsync(request, messageIdResult));

            var rt = new ChatMessageDtoSafe
            {
                Id = messageIdResult.Id.ToString(),
                Content = request.TextContent,
                ContentMedia = request.MediaContent,
                MessageType = (int)MessageType.Text,
                SenderId = request.SenderId,
                ChannelId = request.ReceiveChannelId,
                CreatedAt = messageIdResult.CreatedAt
            };
            await _cache.AddMessageToLatestBucketAsync(long.Parse(request.ReceiveChannelId), rt);
            return OperationResultT<ChatMessageDtoSafe>.Ok(rt);
        }

    }

}
