using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IMessageReadService
    {
        Task<List<BucketDto>> GetLatestBucketsAsync(long channelId, int bucketCount = 3);
        Task<BucketDto?> GetBucketByIdAsync(long channelId, int bucketId);
        Task<BucketDto?> GetBucketContainingMessageAsync(long channelId, long messageId);

    }
}
