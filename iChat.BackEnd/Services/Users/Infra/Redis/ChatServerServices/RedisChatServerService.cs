using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using StackExchange.Redis;

namespace iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices
{
    public class RedisChatServerService
    {
        private readonly AppRedisService _service;
        public RedisChatServerService(AppRedisService redisService)
        {
            _service = redisService;
        }

        public async Task<bool> UploadServerAsync(Dictionary<long, List<long>> serverList, int chunkSize = 100)
        {
            var db = _service.GetDatabase();

            var serverEntries = serverList.ToList();
            for (int i = 0; i < serverEntries.Count; i += chunkSize)
            {
                var chunk = serverEntries.Skip(i).Take(chunkSize);

                var batch = db.CreateBatch();
                var tasks = new List<Task>();

                foreach (var (serverId, channelIdList) in chunk)
                {
                    var key = RedisVariableKey.GetServerChannelKey(serverId);
                    var entries = channelIdList
                        .Select((id, index) => new SortedSetEntry(id.ToString(), index))
                        .ToArray();

                    tasks.Add(batch.SortedSetAddAsync(key, entries));
                }

                batch.Execute();
                await Task.WhenAll(tasks);
            }

            return true;
        }

        public async Task<long?> GetUserPermissionAsync(long userId, long serverId, long channelId)
        {
            var db = _service.GetDatabase();
            var key = RedisVariableKey.GetUserServerPermsKey(userId, serverId);
            var value = await db.HashGetAsync(key, channelId.ToString());
            return value.IsNull ? (long?)null : (long)value;
        }

        public async Task<bool> SetUserPermissionAsync(long userId, long serverId, long channelId, long perm)
        {
            var db = _service.GetDatabase();
            var userKey = RedisVariableKey.GetUserServerPermsKey(userId, serverId);
            var channelKey = RedisVariableKey.GetChannelUserPermsKey(serverId, channelId);

            var batch = db.CreateBatch();
            var task1 = batch.HashSetAsync(userKey, channelId.ToString(), perm);
            var task2 = batch.HashSetAsync(channelKey, userId.ToString(), perm);
            batch.Execute();
            await Task.WhenAll(task1, task2);
            return true;
        }

        public async Task<Dictionary<long, long>> GetAllUserPermsInServerAsync(long userId, long serverId)
        {
            var db = _service.GetDatabase();
            var key = RedisVariableKey.GetUserServerPermsKey(userId, serverId);
            var entries = await db.HashGetAllAsync(key);
            return entries.ToDictionary(e => long.Parse(e.Name!), e => (long)e.Value);
        }

        public async Task<Dictionary<long, long>> GetAllUserPermsInChannelAsync(long serverId, long channelId)
        {
            var db = _service.GetDatabase();
            var key = RedisVariableKey.GetChannelUserPermsKey(serverId, channelId);
            var entries = await db.HashGetAllAsync(key);
            return entries.ToDictionary(e => long.Parse(e.Name!), e => (long)e.Value);
        }
    }
}
