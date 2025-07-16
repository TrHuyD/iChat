using Azure.Core;
using iChat.BackEnd.Models.User;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.BackEnd.Services.Validators.TextMessageValidators;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppMessageWriteService  : IMessageWriteService

    {
        readonly IMessageDbWriteService _chatWriteService;
       // readonly RedisChatCache _redis_dbservice;
        readonly SnowflakeService _idGen;
        IMessageCacheService _cache;
        MemCacheUserChatService _userCacher;
        readonly IChatServerMetadataCacheService _serverMetaDataCacheService;
        public AppMessageWriteService(IMessageDbWriteService dbservice,
            IMessageCacheService cache
        //    ,RedisChatCache rWService
            ,SnowflakeService snowflakeService,
            IChatServerMetadataCacheService ServerMetaDataCache,
            MemCacheUserChatService userCacher)
        {
            _userCacher = userCacher;
             _serverMetaDataCacheService = ServerMetaDataCache;
            _cache = cache;
            _chatWriteService = dbservice;
       //     _redis_dbservice = rWService;
            _idGen = snowflakeService;
        }

        public async Task<DeleteMessageRt> DeleteMessageAsync(UserDeleteMessageRq rq,string UserId)
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
            var bucketId=await _chatWriteService.DeleteMessageAsync(longrq, isAdmin);
            return new DeleteMessageRt
            {
                ChannelId = rq.ChannelId,
                MessageId = rq.MessageId,
                ServerId = rq.ServerId,
                BucketId =bucketId
            };

        }

        public async Task<EditMessageRt> EditMessageAsync(UserEditMessageRq rq, string UserId)
        {
            var longrq= new EditMessageRq
            {
                ChannelId = long.Parse(rq.ChannelId),
                MessageId = long.Parse(rq.MessageId),
                UserId = long.Parse(UserId),
                NewContent = rq.NewContent
            };
            await _serverMetaDataCacheService.IsInServerWithCorrectStruct(longrq.UserId, longrq.UserId, longrq.ChannelId);
            var cacheResult= await _cache.EditMessageAsync(longrq);
            var bucketID =await _chatWriteService.EditMessageAsync(longrq);
            return new EditMessageRt
            {
                ChannelId = rq.ChannelId,
                MessageId = rq.MessageId,
                NewContent = rq.NewContent,
                ServerId = rq.ServerId,
                BucketId = bucketID
            };
        }
        public async Task<NewMessage> SendMediaMessageAsync(MessageUploadRequest rq,string UserId)
        {
            var channelId = long.Parse(rq.ChannelId);
            var serverId = long.Parse(rq.ServerId);
            var userId = long.Parse(UserId);
            await _serverMetaDataCacheService.IsInServerWithCorrectStruct(userId, serverId, channelId);
            var messageIdResult = _idGen.GenerateId();
            var uploadResult = await _chatWriteService.UploadImage(rq, messageIdResult, userId);
            var chatMessage = new ChatMessageDtoSafe
            {
                Id = messageIdResult.Id.ToString(),
                Content="",
                ContentMedia =uploadResult,
                MessageType=(int)MessageType.Media,
                SenderId= UserId,
                ChannelId= rq.ChannelId,
                CreatedAt=messageIdResult.CreatedAt,
            };
            return new NewMessage
            {
                message = chatMessage,
                UserMetadataVersion = _userCacher.GetMetadataVersion(UserId)
            };
        }

        public async Task<OperationResultT<NewMessage>> SendTextMessageAsync(MessageRequest request,string serverId)
        {
            var channelId = long.Parse(request.ReceiveChannelId);
            var serverIdLong = long.Parse(serverId);
            var userId = long.Parse(request.SenderId);
            await _serverMetaDataCacheService.IsInServerWithCorrectStruct(userId, serverIdLong, channelId);
            var messageIdResult = _idGen.GenerateId();
            _ = Task.Run(() => _chatWriteService.UploadMessageAsync(request, messageIdResult));
            var chatMesssage = new ChatMessageDtoSafe
            {
                Id = messageIdResult.Id.ToString(),
                Content = request.TextContent,
                ContentMedia = null,
                MessageType = (int)MessageType.Text,
                SenderId = request.SenderId,
                ChannelId = request.ReceiveChannelId,
                CreatedAt = messageIdResult.CreatedAt,
            };
            var rt = new NewMessage
            {
                message = chatMesssage,
                UserMetadataVersion = _userCacher.GetMetadataVersion(request.SenderId)
            };
            await _cache.AddMessageToLatestBucketAsync(channelId, chatMesssage);
            return OperationResultT<NewMessage>.Ok(rt);
        }

    }

}
