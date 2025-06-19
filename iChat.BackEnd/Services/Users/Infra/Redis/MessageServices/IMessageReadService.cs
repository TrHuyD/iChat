using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public interface IMessageReadService
    {
        Task<List<ChatMessageDto>> GetMessagesByChannelAsync(long channelId, int limit = 40);
        Task<List<ChatMessageDto>> GetMessagesAroundMessageIdAsync(long channelId, long messageId, int before = 20, int after = 22);
        Task<List<ChatMessageDto>> GetMessagesInRangeAsync(long channelId, long startId, long endId, int limit = 50);
    }

}
