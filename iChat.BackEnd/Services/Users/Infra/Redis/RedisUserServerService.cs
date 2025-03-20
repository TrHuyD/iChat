using StackExchange.Redis;

namespace iChat.BackEnd.Services.Users.Infra.Redis
{
    public class RedisUserServerService
    {
        private readonly AppRedisService _service;
        int expiryTime = 86400;
        TimeSpan expiryTimeSpan = TimeSpan.FromSeconds(86400);
        public RedisUserServerService(AppRedisService redisService)
        {
            _service = redisService;
        }
        public async Task<int> CheckIfUserInServer(string userId, string serverId)
        {
            var key = $"u:{userId}:s";
            return await _service.CheckAndExtendMembershipExpiryAsync(key, serverId, expiryTimeSpan);
        }
        public async Task<int> AddUserServersAsync(string userId, IEnumerable<string> serverIds)
        {
            var key = $"u:{userId}:s";
            
            var result = await _service.AddListAsync(key, expiryTime, serverIds);
            return (int)result!;
        }
        public async Task<List<string>?> GetUserServersAsync(string userId)
        {
            var serverKey = $"u:{userId}:s";
            var result = await _service.GetListAsync(serverKey);
            return result;
        }
        public async Task<List<string>?> GetServerChannelsAsync(string serverId)
        {
            var key = $"s:{serverId}:c";
            return await _service.GetListAsync(key);
        }
        public async Task<int> AddServerChannelsAsync(string serverId, IEnumerable<string> channelIds)
        {
            var key = $"s:{serverId}:c";
            int expiryTime = 86400;
            var result = await _service.AddListAsync(key, expiryTime, channelIds);
            return (int)result!;
        }


    }
}
