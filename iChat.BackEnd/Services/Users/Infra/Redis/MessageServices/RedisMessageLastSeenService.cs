using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.Client.Data.Chat;
using StackExchange.Redis;

namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public class RedisMessageLastSeenService : IMessageLastSeenService
    {
        private readonly AppRedisService _redisService;
        public RedisMessageLastSeenService(AppRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<Dictionary<string, string>> GetLastSeenMessageAsync(string serverID, string userId)
        {
            var key = RedisVariableKey.GetLastSeenKey(userId, serverID);
            HashEntry[] entries = await _redisService.GetDatabase().HashGetAllAsync(key);
            var dic = entries.ToDictionary(
                x => x.Name.ToString(),
                x => x.Value.ToString()
            );
            return dic;
        }

        public async Task UpdateLastSeenAsync(string chatChannelId,string serverId, string MessageId, string userId)
        {
            await _redisService.GetDatabase().HashSetAsync(RedisVariableKey.GetLastSeenKey(userId, serverId), chatChannelId, MessageId);

        }
    }
}
