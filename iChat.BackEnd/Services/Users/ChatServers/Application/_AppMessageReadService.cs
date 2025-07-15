using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;

//using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using iChat.BackEnd.Services.Users.Infra.Helpers;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.DTOs.Users.Messages;
using System.Threading.Channels;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{

    //[Obsolete("This service is deprecated and will be removed in future versions. Use the new ChatReadMessageService instead.")]
    //public class _AppMessageReadService : IChatReadMessageService
    //{
    //    private readonly RedisChatCache _redisService;
    //    private readonly IMessageDbReadService _chatReadService;
    //    private readonly ThreadSafeCacheService _lockService;
    //    private readonly RedisSegmentCache _redisSegmentCache;
    //    public _AppMessageReadService(
    //        RedisChatCache redisService,
    //        IMessageDbReadService readservice,
    //        ThreadSafeCacheService lockService,
    //        RedisSegmentCache redisSegmentCache)
    //    {
    //        _redisService = redisService;
    //        _chatReadService = readservice;
    //        _lockService = lockService;
    //        _redisSegmentCache= redisSegmentCache;
    //    }


    //    public async Task<List<ChatMessageDto>> RetrieveRecentMessage(UserGetRecentMessageRequest request)
    //    {
    //        var channelId = request.ChannelId;
            
    //        return await _lockService.GetOrRenewWithLockAsync(
    //            () => _redisService.GetRecentMessage(channelId,request.LastMessageId),
    //            async () => await _chatReadService.GetMessagesByChannelAsync(channelId) ?? throw new Exception("Failed to connect to _cassService"),
    //            async data => await _redisService.UploadMessage_Bulk(channelId, data),
    //            () => RedisVariableKey.GetRecentChatMessageKey_Lock(channelId),
    //            result => result?.Count == 0,
    //            maxRetry: 3,
    //            delayMs: 250
    //        );

    //    }
    //    public async Task<List<ChatMessageDto>> GetMessagesBeforeAsync(long channelId, long messageId)
    //    {
    //        // 1. Try Redis first
    //        var segment = await _redisSegmentCache.TryGetSegmentBefore(channelId, messageId);
    //        if (segment != null && segment.Count > 0)
    //            return segment;
    //        // 2. Fallback to Cassandra
    //        var cassandraMessages = await _chatReadService.GetMessagesAroundMessageIdAsync(
    //            channelId, messageId, before: 42, after: 0);

    //        if (cassandraMessages.Count == 0)
    //            return new List<ChatMessageDto>();
    //        // 3. Check if the requested message ID actually exists in result
    //        if (!cassandraMessages.Any(m => m.Id < messageId))
    //        {
    //            Console.WriteLine($"[Warning] No messages found before ID {messageId} in channel {channelId}");
    //            return new List<ChatMessageDto>();
    //        }
    //        // 4. Sort and cache segment
    //        var sorted = cassandraMessages
    //            .Where(m => m.Id < messageId)
    //            .OrderBy(m => m.Id)
    //            .ToList();

    //       // if (sorted.Count >= 15)
    //            await _redisSegmentCache.UploadSegmentAsync(channelId, sorted);

    //        return sorted;
    //    }

    //    public async Task<List<ChatMessageDto>> GetMessagesContainingAsync(long channelId, long messageId)
    //    {
    //        // 1. Try Redis first
    //        var segment = await _redisSegmentCache.TryGetSegmentContaining(channelId, messageId);
    //        if (segment != null && segment.Count > 0)
    //            return segment;

    //        // 2. Fallback to Cassandra
    //        var cassandraMessages = await _chatReadService.GetMessagesAroundMessageIdAsync(channelId, messageId, before: 20, after: 22);
    //        if (cassandraMessages.Count == 0)
    //            return new List<ChatMessageDto>();

    //        // 3. Check if the requested message ID actually exists in result
    //        if (!cassandraMessages.Any(m => m.Id == messageId))
    //        {
    //            Console.WriteLine($"[Warning] Message ID {messageId} not found in channel {channelId} — possible cross-channel or corrupted jump.");
    //            return new List<ChatMessageDto>();
    //        }

    //        // 4. Sort and cache segment
    //        var sorted = cassandraMessages.OrderBy(m => m.Id).ToList();
    //      //  if (sorted.Count >= 15)
    //            await _redisSegmentCache.UploadSegmentAsync(channelId, sorted);

    //        return sorted;
    //    }


    //    public async Task<List<(long Start, long End)>> GetCachedRanges(long channelId)
    //    {
    //        return await _redisSegmentCache.GetCachedSegments(channelId);
    //    }

    //    public async Task<bool> IsMessageCached(long channelId, long messageId)
    //    {
    //        return await _redisSegmentCache.IsMessageCached(channelId, messageId);
    //    }
    //}
}
