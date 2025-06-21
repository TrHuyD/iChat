using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;
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
        void AddEnd(List<ChatMessageDto> list,int limit)
        {
            if(list.Count<limit)
                list.Insert(0, new ChatMessageDto
                {
                    Id = ValueParser.MinValidId,
                    SenderId = 0,
                    MessageType = (int)MessageType.EndOfHistory,
                    Content = "End of chat",
                    CreatedAt = DateTime.MinValue,
                });
        }
        public async Task<List<ChatMessageDto>> GetMessagesByChannelAsync(long channelId, int limit = 40)
        {
            var result= await  _context.Messages
                .AsNoTracking()
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.Id)
                .Take(limit)
                .Select(_toDto)
                .ToListAsync();
            AddEnd(result,limit);
            return result;
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
            if (before.Count == 0)
                return new();
            var after = afterTask.Result;
            before.Reverse();
            AddEnd(before, beforeIndex + 1);
            return before.Concat(after).ToList();
        }

        public async Task<List<ChatMessageDto>> GetMessagesInRangeAsync(long channelId, long startId, long endId, int limit = 50)
        {
            var result = await _context.Messages
                .AsNoTracking()
                .Where(m => m.ChannelId == channelId && m.Id >= startId && m.Id <= endId)
                .OrderBy(m => m.Id)
                .Take(limit)
                .Select(_toDto)
                .ToListAsync();
            return result;
         //   AddEnd(result, limit);
        }
    }
}
