using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.DTOs.Users.Messages;
using StackExchange.Redis;
using System.Text.Json;

namespace iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices
{
    public class RedisChatServerService
    {
        private readonly AppRedisService _service;
        public RedisChatServerService(AppRedisService redisService)
        {
            _service = redisService;
        }

        public async Task<bool> UploadServersAsync(List<ChatServerMetadata> servers, int chunkSize = 100)
        {
            try
            {
                var db = _service.GetDatabase();
                for (int i = 0; i < servers.Count; i += chunkSize)
                {
                    var chunk = servers.Skip(i).Take(chunkSize);
                    var batch = db.CreateBatch();
                    var tasks = new List<Task>();
                    foreach (var server in chunk)
                    {
                        var serverMeta = new
                        {
                            server.Id,
                            server.Name,
                            server.AvatarUrl,
                            server.CreatedAt
                        };
                        var serverKey = RedisVariableKey.GetServerMetadataKey(server.Id);
                        tasks.Add(batch.StringSetAsync(serverKey, JsonSerializer.Serialize(serverMeta)));
                        if (server.Channels.Any())
                        {
                            var channelHash = new List<HashEntry>();
                            foreach (var channel in server.Channels)
                            {
                                channelHash.Add(new HashEntry($"{channel.Id}:name", channel.Name));
                                channelHash.Add(new HashEntry($"{channel.Id}:order", channel.Order.ToString()));
                                channelHash.Add(new HashEntry($"{channel.Id}:last_bucket_id", channel.last_bucket_id.ToString()));
                            }
                            var channelKey = RedisVariableKey.GetServerChannelsKey(server.Id);
                            tasks.Add(batch.HashSetAsync(channelKey, channelHash.ToArray()));
                        }
                    }
                    batch.Execute();
                    await Task.WhenAll(tasks);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to upload servers to Redis: {ex.Message}");
                return false;
            }
        }
        public async Task<ChatServerMetadata?> GetServerFromRedisAsync(string serverId)
        {
            try
            {
                var db = _service.GetDatabase();
                var serverKey = RedisVariableKey.GetServerMetadataKey(serverId);
                var serverMetaString = await db.StringGetAsync(serverKey);
                if (serverMetaString.IsNullOrEmpty)
                    return null;
                var serverMeta = JsonSerializer.Deserialize<ChatServerMetadata>(serverMetaString!);
                if (serverMeta == null)
                    return null;
                var channelKey = RedisVariableKey.GetServerChannelsKey(serverId);
                var hashEntries = await db.HashGetAllAsync(channelKey);
                if (hashEntries.Length == 0)
                {
                    serverMeta.Channels = new List<ChatChannelMetadata>();
                    return serverMeta;
                }
                var channelMap = new Dictionary<string, ChatChannelMetadata>();
                foreach (var entry in hashEntries)
                {
                    var parts = entry.Name.ToString().Split(':');
                    if (parts.Length != 2) continue;

                    var channelId = parts[0];
                    var field = parts[1];
                    if (!channelMap.TryGetValue(channelId, out var channel))
                    {
                        channel = new ChatChannelMetadata { Id = channelId };
                        channelMap[channelId] = channel;
                    }
                    var value = entry.Value.ToString();
                    switch (field)
                    {
                        case "name":
                            channel.Name = value;
                            break;
                        case "order":
                            if (int.TryParse(value, out var order)) channel.Order = order;
                            break;
                        case "last_bucket_id":
                            if (int.TryParse(value, out var bucketId)) channel.last_bucket_id = bucketId;
                            break;
                    }
                }

                serverMeta.Channels = channelMap.Values.OrderBy(c => c.Order).ToList();
                return serverMeta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get server from Redis: {ex.Message}");
                return null;
            }
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
