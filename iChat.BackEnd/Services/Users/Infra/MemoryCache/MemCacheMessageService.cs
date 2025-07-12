using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;
namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class MemCacheMessageService : IMessageCacheService
    {
        private readonly IMemoryCache _cache;

        private static string BucketKey(long channelId, int bucketId) => $"bucket:{channelId}:{bucketId}";
        private static string AllBucketsKey(long channelId) => $"bucket:all:{channelId}";
        private static string LatestBucketsKey(long channelId) => $"bucket:latest:{channelId}";

        public MemCacheMessageService(IMemoryCache cache)
        {
            _cache = cache;
        }
        public Task<BucketDto?> GetCachedBucketAsync(long channelId, int bucketId)
            => Task.FromResult(_cache.TryGetValue(BucketKey(channelId, bucketId), out BucketDto bucket) ? bucket : null);
        public Task SetBucketAsync(long channelId, int bucketId, BucketDto bucket)
        {
            var bucketKey = BucketKey(channelId, bucketId);
            var allBucketsKey = AllBucketsKey(channelId);
            var expirationToken = new CancellationTokenSource();
            var bucketOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            if (_cache.TryGetValue(allBucketsKey, out SortedSet<int> bucketSet))
                            {
                                lock (bucketSet)
                                {
                                    bucketSet.Remove(bucketId);
                                }
                            }
                        }
                    }
                }
            };
            _cache.Set(bucketKey, bucket, bucketOptions);
            var bucketSet = _cache.GetOrCreate(allBucketsKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(15);
                return new SortedSet<int>();
            });
            lock (bucketSet)
            {
                bucketSet.Add(bucketId);
            }

            return Task.CompletedTask;
        }

        public Task<List<BucketDto>> GetCachedLatestBucketsAsync(long channelId, int count)
        {
            if (_cache.TryGetValue(LatestBucketsKey(channelId), out List<BucketDto> buckets))
                return Task.FromResult(buckets.Take(count).ToList());

            return Task.FromResult(new List<BucketDto>());
        }

        public Task SetLatestBucketsAsync(long channelId, List<BucketDto> buckets)
        {
            _cache.Set(LatestBucketsKey(channelId), buckets, TimeSpan.FromSeconds(20));
            foreach (var bucket in buckets)
            {
                SetBucketAsync(channelId, bucket.BucketId, bucket);
            }
            return Task.CompletedTask;
        }

        public Task<BucketDto?> GetBucketContainingMessageAsync(long channelId, long messageId)
        {
            if (!_cache.TryGetValue(AllBucketsKey(channelId), out SortedSet<int> bucketIds))
                return Task.FromResult<BucketDto?>(null);

            lock (bucketIds)
            {
                foreach (var bucketId in bucketIds.ToList())
                {
                    if (_cache.TryGetValue(BucketKey(channelId, bucketId), out BucketDto bucket))
                    {
                        if (long.TryParse(bucket.FirstSequence, out long firstId) &&
                            long.TryParse(bucket.LastSequence, out long lastId))
                        {
                            if (messageId >= firstId && messageId <= lastId)
                            {
                                return Task.FromResult<BucketDto?>(bucket);
                            }
                        }
                    }
                    else
                    {
                        bucketIds.Remove(bucketId);
                    }
                }
            }
            return Task.FromResult<BucketDto?>(null);
        }

        public Task<bool> EditMessageAsync(EditMessageRq rq)
        {
            var bucket = GetBucketContainingMessageAsync(rq.ChannelId, rq.MessageId).Result;
            if (bucket == null) return Task.FromResult(false);
            var message = FindMessageInBucket(bucket, rq.MessageId);
            if (message != null)
            {
                if(message.isDeleted)
                    throw new InvalidOperationException("Cannot edit a deleted message");
                if (message.SenderId != rq.UserId.ToString())
                    throw new InvalidOperationException("Not authorized to edit this message");
                message.Content = rq.NewContent;
                message.isEdited = true;
                SetBucketAsync(rq.ChannelId, bucket.BucketId, bucket);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteMessageAsync(DeleteMessageRq rq, bool hasAdminPerm)
        {
            var bucket = GetBucketContainingMessageAsync(rq.ChannelId, rq.MessageId).Result;
            if (bucket == null) return Task.FromResult(false);
            var message = FindMessageInBucket(bucket, rq.MessageId);
            if (message != null)
            {
                if(!hasAdminPerm && message.SenderId != rq.UserId.ToString())
                   throw new InvalidOperationException("Not authorized to delete this message");
                if(message.isDeleted)
                    throw new InvalidOperationException("Message already deleted");
                message.Content = "Message deleted";
                message.isDeleted = true;
                SetBucketAsync(rq.MessageId, bucket.BucketId, bucket);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        public Task AddMessageToLatestBucketAsync(long channelId, ChatMessageDtoSafe message)
        {
            var latestBuckets = GetCachedLatestBucketsAsync(channelId, 1).Result;
            if (latestBuckets.Any())
            {
                var latestBucket = latestBuckets.First();
                latestBucket.ChatMessageDtos.Add(message);
                latestBucket.LastSequence = message.Id;

                // Update the bucket in cache
                SetBucketAsync(channelId, latestBucket.BucketId, latestBucket);
            }

            return Task.CompletedTask;
        }

        private ChatMessageDtoSafe? FindMessageInBucket(BucketDto bucket, long messageId)
        {
            if (bucket.ChatMessageDtos == null || !bucket.ChatMessageDtos.Any())
                return null;

            int left = 0;
            int right = bucket.ChatMessageDtos.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                var currentMessage = bucket.ChatMessageDtos[mid];

                if (long.TryParse(currentMessage.Id, out long currentId))
                {
                    if (currentId == messageId)
                        return currentMessage;
                    else if (currentId < messageId)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }
                else
                {
                    return bucket.ChatMessageDtos.FirstOrDefault(m => m.Id == messageId.ToString());
                }
            }

            return null;
        }
    }
    }