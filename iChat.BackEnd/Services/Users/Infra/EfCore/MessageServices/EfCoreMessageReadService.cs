using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users.Messages;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
namespace iChat.BackEnd.Services.Users.Infra.EFcore.MessageServices
{
    public class EfCoreMessageReadService : IMessageReadService
    {
        private readonly iChatDbContext _context;

        public EfCoreMessageReadService(iChatDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessageDto>> GetMessagesByChannelAsync(long channelId, int limit = 40)
        {
            return await _context.Messages
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.Id)
                .Take(limit)
                .Select(ToDto())
                .ToListAsync();
        }

        public async Task<List<ChatMessageDto>> GetMessagesAroundMessageIdAsync(long channelId, long messageId, int before = 20, int after = 22)
        {
            var beforeMessages = await _context.Messages
                .Where(m => m.ChannelId == channelId && m.Id <= messageId)
                .OrderByDescending(m => m.Id)
                .Take(before + 1)
                .Select(ToDto())
                .ToListAsync();

            var afterMessages = await _context.Messages
                .Where(m => m.ChannelId == channelId && m.Id > messageId)
                .OrderBy(m => m.Id)
                .Take(after)
                .Select(ToDto())
                .ToListAsync();

            beforeMessages.Reverse();
            return beforeMessages.Concat(afterMessages).ToList();
        }

        public async Task<List<ChatMessageDto>> GetMessagesInRangeAsync(long channelId, long startId, long endId, int limit = 50)
        {
            return await _context.Messages
                .Where(m => m.ChannelId == channelId && m.Id >= startId && m.Id <= endId)
                .OrderBy(m => m.Id)
                .Take(limit)
                .Select(ToDto())
                .ToListAsync();
        }

        private static Expression<Func<Message, ChatMessageDto>> ToDto()
        {
            return m => new ChatMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                MessageType = m.MessageType,
                Content = m.TextContent ?? string.Empty,
                ContentMedia = m.MediaContent ?? string.Empty,
                CreatedAt = m.Timestamp
            };
        }
    }
}
