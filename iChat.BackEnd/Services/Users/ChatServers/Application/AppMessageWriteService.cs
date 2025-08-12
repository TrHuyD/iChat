using Azure.Core;
using iChat.BackEnd.Collections;
using iChat.BackEnd.Models.User;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.BackEnd.Services.Validators.TextMessageValidators;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;
using Microsoft.AspNetCore.Http.HttpResults;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppMessageWriteService  : IMessageWriteService

    {
        readonly IMessageDbWriteService _chatWriteService;
       // readonly RedisChatCache _redis_dbservice;
        readonly SnowflakeService _idGen;
        IMessageCacheService _cache;
        MemCacheUserChatService _userCacher;
        readonly IPermissionService _permService;
        public AppMessageWriteService(IMessageDbWriteService dbservice,
            IMessageCacheService cache
        //    ,RedisChatCache rWService
            ,SnowflakeService snowflakeService,
            IPermissionService ServerMetaDataCache,
            MemCacheUserChatService userCacher)
        {
            _userCacher = userCacher;
            _permService = ServerMetaDataCache;
            _cache = cache;
            _chatWriteService = dbservice;
       //     _redis_dbservice = rWService;
            _idGen = snowflakeService;
        }

        public async Task<OperationResultT<DeleteMessageRt>> DeleteMessageAsync(UserDeleteMessageRq rq,string _userId)
        {
            var channelId = new ChannelId(new stringlong(rq.ChannelId));
            var serverId = new ServerId(new stringlong(rq.ServerId));
            var userId = new UserId(new stringlong(_userId));
            var longrq= new DeleteMessageRq
            {
                ChannelId =channelId,
                MessageId =long.Parse( rq.MessageId),
                UserId =userId,
                ServerId=serverId
            };

            var isAdminRt =  _permService.IsAdmin(serverId,channelId,userId);
            if(!isAdminRt.Success)
                return OperationResultT<DeleteMessageRt>.Fail(isAdminRt.ErrorCode,"Error when deleting message "+isAdminRt.ErrorMessage);
            var isAdmin = isAdminRt.Value;
            var cacheResult =await _cache.DeleteMessageAsync(longrq, isAdmin);
            var bucketId=await _chatWriteService.DeleteMessageAsync(longrq, isAdmin);
            return OperationResultT<DeleteMessageRt>.Ok( new DeleteMessageRt
            {
                ChannelId = rq.ChannelId,
                MessageId = rq.MessageId,
                ServerId = rq.ServerId,
                BucketId =bucketId
            });

        }

        public async Task<OperationResultT<EditMessageRt>> EditMessageAsync(UserEditMessageRq rq, string _userId)
        {
            var channelId = new ChannelId(new stringlong(rq.ChannelId));
            var serverId = new ServerId(new stringlong(rq.ServerId));
            var userId = new UserId(new stringlong(_userId));
            var longrq= new EditMessageRq
            {
                ChannelId = channelId,
                MessageId = long.Parse(rq.MessageId),
                UserId = userId,
                NewContent = rq.NewContent
            };
            var isAdminRt =  _permService.IsAdmin(serverId, channelId, userId);
            if (!isAdminRt.Success)
                return OperationResultT<EditMessageRt>.Fail(isAdminRt.ErrorCode, "Error when editing message :" + isAdminRt.ErrorCode);
            var cacheResult= await _cache.EditMessageAsync(longrq);
            var bucketID =await _chatWriteService.EditMessageAsync(longrq);
            return OperationResultT < EditMessageRt >.Ok( new EditMessageRt
            {
                ChannelId = rq.ChannelId,
                MessageId = rq.MessageId,
                NewContent = rq.NewContent,
                ServerId = rq.ServerId,
                BucketId = bucketID
            });
        }
        public async Task<OperationResultT<NewMessage>> SendMediaMessageAsync(MessageUploadRequest rq,string UserId,bool fromApi)
        {
            var channelId =new ChannelId(  new stringlong(rq.ChannelId));
            var serverId = new ServerId(new stringlong(rq.ServerId));
            var userId = new UserId( new stringlong(UserId));
            if (fromApi)
            {
                var isAdminRt =  _permService.IsAdmin(serverId, channelId, userId);
                if (!isAdminRt.Success)
                    return OperationResultT<NewMessage>.Fail(isAdminRt.ErrorCode, "Error when editing message :" + isAdminRt.ErrorCode);
            }
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
            return OperationResultT < NewMessage > .Ok(new NewMessage
            {
                message = chatMessage,
                UserMetadataVersion = _userCacher.GetMetadataVersion(UserId)
            });
        }

        public async Task<OperationResultT<NewMessage>> SendTextMessageAsync(MessageRequest request,string _serverId,bool fromApi)
        {
            var channelId = new ChannelId(new stringlong(request.ReceiveChannelId));
            var serverId = new ServerId(new stringlong(_serverId));
            var userId = new UserId(new stringlong(request.SenderId));
            if (fromApi)
            {
                var permCheck =  _permService.IsAdmin(serverId, channelId, userId);
                if (!permCheck.Success)
                    return OperationResultT<NewMessage>.Fail(permCheck.ErrorCode, permCheck.ErrorMessage);
            }
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
