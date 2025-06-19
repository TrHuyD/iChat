using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users.Messages;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace iChat.BackEnd.Services.Users.Infra.EFcore.MessageServices
{
    public class EfCoreMessageReadService : IMessageReadService
    {
        private readonly iChatDbContext _context;

        private static readonly Expression<Func<Message, ChatMessageDto>> _toDto = m => new ChatMessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            MessageType = m.MessageType,
            Content = m.TextContent ?? "",
            ContentMedia = m.MediaContent ?? "",
            CreatedAt = m.Timestamp
        };

        public EfCoreMessageReadService(iChatDbContext context)
        {
            _context = context;
        }

        public Task<List<ChatMessageDto>> GetMessagesByChannelAsync(long channelId, int limit = 40)
        {
            return _context.Messages
                .AsNoTracking()
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.Id)
                .Take(limit)
                .Select(_toDto)
                .ToListAsync();
        }

        public async Task<List<ChatMessageDto>> GetMessagesAroundMessageIdAsync(long channelId, long messageId, int beforeIndex = 20, int AfterIndex = 22)
        {
            var beforeTask = _context.Messages
                .AsNoTracking()
                .Where(m => m.ChannelId == channelId && m.Id <= messageId)
                .OrderByDescending(m => m.Id)
                .Take(beforeIndex + 1)
                .Select(_toDto)
                .ToListAsync();

            var afterTask = _context.Messages
                .AsNoTracking()
                .Where(m => m.ChannelId == channelId && m.Id > messageId)
                .OrderBy(m => m.Id)
                .Take(AfterIndex)
                .Select(_toDto)
                .ToListAsync();

            await Task.WhenAll(beforeTask, afterTask);

            var before = beforeTask.Result;
            var after = afterTask.Result;
            before.Reverse();

            return before.Concat(after).ToList();
        }

        public Task<List<ChatMessageDto>> GetMessagesInRangeAsync(long channelId, long startId, long endId, int limit = 50)
        {
            return _context.Messages
                .AsNoTracking()
                .Where(m => m.ChannelId == channelId && m.Id >= startId && m.Id <= endId)
                .OrderBy(m => m.Id)
                .Take(limit)
                .Select(_toDto)
                .ToListAsync();
        }
    }
}
