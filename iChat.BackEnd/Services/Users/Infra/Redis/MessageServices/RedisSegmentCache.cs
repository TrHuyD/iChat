using iChat.DTOs.Users.Messages;
using Newtonsoft.Json;

namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public class RedisSegmentCache
    {
        private readonly AppRedisService _service;
        private const int SegmentTtlSeconds = 1200;

        public RedisSegmentCache(AppRedisService redisService)
        {
            _service = redisService;
        }

        private static string GetSegmentKey(long channelId, long startId, long endId)
            => $"c:{channelId}:{startId}~{endId}";

        private static string GetSegmentIndexKey(long channelId)
            => $"c:{channelId}:segments";

        public async Task<bool> UploadSegmentAsync(long channelId, List<ChatMessageDto> messages)
        {
            if (messages == null || messages.Count == 0) return false;

            var db = _service.GetDatabase();

            var sortedMessages = messages.OrderBy(m => m.Id).ToList();
            long startId = sortedMessages.First().Id;
            long endId = sortedMessages.Last().Id;

            string segmentKey = GetSegmentKey(channelId, startId, endId);
            string indexKey = GetSegmentIndexKey(channelId);

            var batch = db.CreateBatch();
            var tasks = new List<Task>();

            foreach (var message in sortedMessages)
            {
                string json = JsonConvert.SerializeObject(message);
                tasks.Add(batch.SortedSetAddAsync(segmentKey, json, message.Id));
            }

            // TTL for this segment
            tasks.Add(batch.KeyExpireAsync(segmentKey, TimeSpan.FromSeconds(SegmentTtlSeconds)));

            // Add to segment index (score = startId)
            tasks.Add(batch.SortedSetAddAsync(indexKey, $"{startId}~{endId}", startId));
            tasks.Add(batch.KeyExpireAsync(indexKey, TimeSpan.FromSeconds(SegmentTtlSeconds)));

            batch.Execute();
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<List<ChatMessageDto>> TryGetSegmentContaining(long channelId, long messageId)
        {
            var db = _service.GetDatabase();
            string indexKey = GetSegmentIndexKey(channelId);

            var allRanges = await db.SortedSetRangeByScoreWithScoresAsync(indexKey);

            foreach (var entry in allRanges)
            {
                var range = entry.Element.ToString();
                var parts = range.Split('~');
                if (parts.Length != 2) continue;

                if (long.TryParse(parts[0], out long start) && long.TryParse(parts[1], out long end))
                {
                    if (messageId >= start && messageId <= end)
                    {
                        var segmentKey = GetSegmentKey(channelId, start, end);
                        var rawMessages = await db.SortedSetRangeByScoreAsync(segmentKey, start, end);
                        return rawMessages.Select(m => JsonConvert.DeserializeObject<ChatMessageDto>(m)).ToList();
                    }
                }
            }

            return null;
        }
        public async Task<List<ChatMessageDto>> TryGetSegmentBefore(long channelId, long messageId)
        {
            var db = _service.GetDatabase();
            string indexKey = GetSegmentIndexKey(channelId);
            var allRanges = await db.SortedSetRangeByScoreWithScoresAsync(indexKey);
            var candidateSegments = new List<(long start, long end)>();
            foreach (var entry in allRanges)
            {
                var range = entry.Element.ToString();
                var parts = range.Split('~');
                if (parts.Length != 2)
                    continue;
                if (long.TryParse(parts[0], out long start) && long.TryParse(parts[1], out long end))
                {
                    if (end < messageId)
                    {
                        candidateSegments.Add((start, end));
                    }
                }
            }
            if (!candidateSegments.Any())
                return null;
            var bestSegment = candidateSegments.OrderByDescending(seg => seg.end).First();
            var segmentKey = GetSegmentKey(channelId, bestSegment.start, bestSegment.end);
            var rawMessages = await db.SortedSetRangeByScoreAsync(segmentKey, bestSegment.start, bestSegment.end);

            return rawMessages.Select(m => JsonConvert.DeserializeObject<ChatMessageDto>(m)).ToList();
        }

        public async Task<List<(long startId, long endId)>> GetCachedSegments(long channelId)
        {
            var db = _service.GetDatabase();
            string indexKey = GetSegmentIndexKey(channelId);

            var allRanges = await db.SortedSetRangeByScoreAsync(indexKey);
            var segments = new List<(long, long)>();

            foreach (var range in allRanges)
            {
                var parts = range.ToString().Split('~');
                if (parts.Length == 2 &&
                    long.TryParse(parts[0], out long start) &&
                    long.TryParse(parts[1], out long end))
                {
                    segments.Add((start, end));
                }
            }

            return segments;
        }

        public async Task<bool> IsMessageCached(long channelId, long messageId)
        {
            var segment = await TryGetSegmentContaining(channelId, messageId);
            return segment != null;
        }
    }

}
