using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public interface IMessageDbReadService
    {
        Task<List<ChatMessageDto>> GetMessagesByChannelAsync(long channelId, int limit = 40);
        Task<List<ChatMessageDto>> GetMessagesAroundMessageIdAsync(long channelId, long messageId, int before = 20, int after = 22);
        Task<List<ChatMessageDto>> GetMessagesInRangeAsync(long channelId, long startId, long endId, int limit = 50);
       Task<List<BucketDto>> GetBucketsInRangeAsync(long channelId, long startId, long endId, int limit = 50);
       Task<List<BucketDto>> GetBucketsAroundMessageAsync(long channelId, long messageId, int bucketRange = 2);
       Task<List<BucketDto>> GetLatestBucketsByChannelAsync(long channelId, int bucketCount = 3);
    }

}
