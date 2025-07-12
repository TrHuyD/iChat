using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public interface IMessageDbReadService
    {
        Task<List<ChatMessageDto>> GetMessagesByChannelAsync(long channelId, int limit = 40);//unused, just ignore
        Task<List<ChatMessageDto>> GetMessagesAroundMessageIdAsync(long channelId, long messageId, int before = 20, int after = 22);//unsed
        Task<List<ChatMessageDto>> GetMessagesInRangeAsync(long channelId, long startId, long endId, int limit = 50);//unused
        Task<List<BucketDto>> GetBucketsInRangeAsync(long channelId, long startId, long endId, int limit = 50);//unsued, just ignore
       Task<List<BucketDto>> GetBucketsAroundMessageAsync(long channelId, long messageId, int bucketRange = 1);//unsued
        Task<List<BucketDto>> GetLatestBucketsByChannelAsync(long channelId, int bucketCount = 3);
        Task<BucketDto> GetBucketById(long channelId, int bucketId);
    }

}
