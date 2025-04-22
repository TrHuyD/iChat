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

    }
}
