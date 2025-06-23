using Azure.Core;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.DTOs.Users.Messages;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public class RedisChatCache
    {
        private readonly AppRedisService _service;
        public RedisChatCache(AppRedisService redisService)
        {
            _service = redisService;
        }
        public async Task<bool> UploadMessageAsync(ChatMessageDto message)
        {
            if (message == null)
            {
                return false;
            }
            var db = _service.GetDatabase();
            var channelKey = (RedisKey)$"c:{message.RoomId}:m";
            var json = JsonConvert.SerializeObject(message);
            var ttlSeconds = 1200;
            var script = @"
                local zset_key = KEYS[1]
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
            try
            {
                var result = await db.ScriptEvaluateAsync(
                    script,
                    new RedisKey[] { channelKey },
                    new RedisValue[] { json, message.Id.ToString(), ttlSeconds.ToString() }
                );
                return (int)result == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadMessageAsync: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> UploadMessage_Bulk(long channelId, List<ChatMessageDto> messages)
        {
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
                    CreatedAt = DateTimeOffset.UtcNow,
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


        public async Task<List<ChatMessageDto>> GetRecentMessage(long channelId,long? lastMessageId)
        {

            var db = _service.GetDatabase();
            RedisValue[] rawMessages; 
            if (lastMessageId is null || lastMessageId < 1000000000000000l)
                 rawMessages = await db.SortedSetRangeByRankAsync(RedisVariableKey.GetRecentChatMessageKey(channelId), -40, -1) ;
            else
                rawMessages = await db.SortedSetRangeByScoreAsync(
                RedisVariableKey.GetRecentChatMessageKey(channelId),
                start:  (double)lastMessageId, 
                stop: double.PositiveInfinity,
                exclude: Exclude.None, 
                order: Order.Ascending
            );

            var messages = rawMessages
                    .Select(m => JsonConvert.DeserializeObject<ChatMessageDto>(m))
                    .ToList();
            return messages;
        }

        public async Task<List<ChatMessageDto>> GetMessagesAfterId(long channelId, long afterSnowflakeId)
        {
            var db = _service.GetDatabase();
            RedisValue[] rawMessages = await db.SortedSetRangeByScoreAsync(
                RedisVariableKey.GetRecentChatMessageKey(channelId),
                start: afterSnowflakeId , 
                stop: long.MaxValue,
                exclude: Exclude.Start,
                order: Order.Ascending
            );
            var messages = rawMessages
                .Select(m => JsonConvert.DeserializeObject<ChatMessageDto>(m))
                .ToList();

            return messages;
        }
    }
}
