using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.DTOs.Users.Messages;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Threading.Tasks;


namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public class RedisMessageRWService
    {
        private readonly AppRedisService _service;
        public RedisMessageRWService(AppRedisService redisService)
        {
            _service = redisService;
        }
        public async Task<bool> UploadMessageAsync(long messageId, ChatMessageDto message)
        {
            var db = _service.GetDatabase();
            var channelId = message.RoomId;
            var json = JsonConvert.SerializeObject(message);
            var ttlSeconds = 1200;

            var script = @"
                local zset_key = 'c:' .. KEYS[1] .. ':m'
                local json = ARGV[1]
                local score = tonumber(ARGV[2])
                local expire = tonumber(ARGV[3])

                if redis.call('EXISTS', zset_key) == 1 then
                    redis.call('ZADD', zset_key, score, json)
                    redis.call('ZREMRANGEBYRANK', zset_key, 0, -41)
                    redis.call('EXPIRE', zset_key, expire)
                    return 1
                else
                    return 0
                end";

            var prepared = LuaScript.Prepare(script);
            var result = (int)(long)(await db.ScriptEvaluateAsync(prepared, new
            {
                KEYS = new RedisKey[] { channelId.ToString() },
                ARGV = new RedisValue[] { json, messageId, ttlSeconds }
            }));

            return result == 1;
        }

        public async Task<bool> UploadMessage_Bulk(string channelId, List<ChatMessageDto> messages)
        {
            if (string.IsNullOrWhiteSpace(channelId) || messages == null)
                return false;

            var db = _service.GetDatabase();
            var zsetKey = RedisVariableKey.GetRecentChatMessageKey(channelId);

            var batch = db.CreateBatch();
            var tasks = new List<Task>();

            if (messages.Count == 0)
            {
                var padding = new ChatMessageDto
                {
                    Id = -1,
                    Content = string.Empty,
                    ContentMedia = string.Empty,
                    MessageType = -1,
                    CreatedAt = DateTimeOffset.Now,
                    SenderId = -1,
                };

                var json = JsonConvert.SerializeObject(padding);
                tasks.Add(batch.SortedSetAddAsync(zsetKey, json, padding.Id));
            }
            else
            {
                foreach (var msg in messages)
                {
                    var json = JsonConvert.SerializeObject(msg);
                    tasks.Add(batch.SortedSetAddAsync(zsetKey, json, msg.Id));
                }
            }
            tasks.Add(batch.SortedSetRemoveRangeByRankAsync(zsetKey, 0, -41));
            tasks.Add(batch.KeyExpireAsync(zsetKey, TimeSpan.FromMinutes(20)));
            batch.Execute();
            await Task.WhenAll(tasks);

            return true;
        }


        public async Task<List<ChatMessageDto>> GetRecentMessage(string channelId)
        {
            var db = _service.GetDatabase();
            RedisValue[] rawMessages = await db.SortedSetRangeByRankAsync(RedisVariableKey.GetRecentChatMessageKey(channelId), -40, -1) ;
            
                //return new List<ChatMessageDto>();
            var messages = rawMessages
                .Select(m => JsonConvert.DeserializeObject<ChatMessageDto>(m))
                .ToList();

            return messages;
        }


    }
}
