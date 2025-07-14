using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public class MessageReadApplicationService : IMessageReadService
    {
        private readonly IMessageCacheService _cacheService;
        private readonly IMessageDbReadService _dbService;

        public MessageReadApplicationService(IMessageCacheService cacheService, IMessageDbReadService dbService)
        {
            _cacheService = cacheService;
            _dbService = dbService;
        }
        public async Task<List<BucketDto>> GetLatestBucketsAsync(long channelId, int bucketCount = 3)
        {
            var cachedBuckets = await _cacheService.GetCachedLatestBucketsAsync(channelId, bucketCount);
            if (cachedBuckets.Any())
            {
                return cachedBuckets;
            }
            var dbBuckets = await _dbService.GetLatestBucketsByChannelAsync(channelId, bucketCount);
            if (dbBuckets.Any())
            {
                await _cacheService.SetLatestBucketsAsync(channelId, dbBuckets);
                return dbBuckets;
            }
            return new List<BucketDto>();
        }

        public async Task<BucketDto?> GetBucketByIdAsync(long channelId, int bucketId)
        {
            // Try cache first
            var cachedBucket = await _cacheService.GetCachedBucketAsync(channelId, bucketId);
            if (cachedBucket != null)
            {
                return cachedBucket;
            }
            // Fallback to database
            var dbBucket = await _dbService.GetBucketById(channelId, bucketId);
            if (dbBucket != null)
            {
                // Cache the result
                await _cacheService.SetBucketAsync(channelId, bucketId, dbBucket);
                return dbBucket;
            }

            return null;
        }

        public async Task<BucketDto?> GetBucketContainingMessageAsync(long channelId, long messageId)
        {
            var cachedBuckets = await _cacheService.GetBucketContainingMessageAsync(channelId, messageId);
            if (cachedBuckets != null)
            {
                return cachedBuckets;
            }
            var dbBucket = await _dbService.GetBucketsAroundMessageAsync(channelId, messageId);
            if(dbBucket != null && dbBucket.Any())
            {
                await _cacheService.SetBucketAsync(channelId, dbBucket[0].BucketId, dbBucket[0]);
                return dbBucket[0];
            }
            return null;
            
        }


        public async Task AddMessageToLatestBucketAsync(long channelId, ChatMessageDtoSafe message)
        {
            await _cacheService.AddMessageToLatestBucketAsync(channelId, message);
        }
    }
}