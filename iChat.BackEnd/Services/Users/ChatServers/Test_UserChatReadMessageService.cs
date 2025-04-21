using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using iChat.BackEnd.Services.Users.Infra.Helpers;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.DTOs.Users.Messages;
using System.Threading.Channels;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public class Test_UserChatReadMessageService : IChatReadMessageService
    {
        private readonly RedisMessageRWService _redisService;
        private readonly CassandraMessageReadService _cassService;
        private readonly ThreadSafeCacheService _lockService;
        public Test_UserChatReadMessageService(
            RedisMessageRWService redisService, 
            CassandraMessageReadService cassService,
            ThreadSafeCacheService lockService)
        {
            _redisService = redisService;
            _cassService = cassService;
            _lockService = lockService;
        }


        public async Task<List<ChatMessageDto>> RetrieveRecentMessage(UserGetRecentMessageRequest request)
        {
            var channelId = request.ChannelId;
            return await _lockService.GetOrRenewWithLockAsync(
                () => _redisService.GetRecentMessage(channelId),
                async () => await _cassService.GetMessagesByChannelAsync(channelId) ?? throw new Exception("Failed to connect to _cassService"),
                async data => await _redisService.UploadMessage_Bulk(channelId, data),
                () => RedisVariableKey.GetRecentChatMessageKey_Lock(channelId),
                result => result?.Count == 0,
                maxRetry: 3,
                delayMs: 250
            );
        }
    }
}
