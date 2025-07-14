using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{

    public interface IMessageCacheService
    {
        Task<BucketDto?> GetCachedBucketAsync(long channelId, int bucketId);
        Task SetBucketAsync(long channelId, int bucketId, BucketDto bucket);
        Task<List<BucketDto>> GetCachedLatestBucketsAsync(long channelId, int count);
        Task SetLatestBucketsAsync(long channelId, List<BucketDto> buckets);
        Task<BucketDto?> GetBucketContainingMessageAsync(long channelId, long messageId);
        Task<bool> EditMessageAsync(EditMessageRq rq);
        Task<bool> DeleteMessageAsync(DeleteMessageRq rq,bool hasAdminPerm);
        Task AddMessageToLatestBucketAsync(long channelId, ChatMessageDtoSafe message);
    }
}
