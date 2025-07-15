using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Linq.Expressions;

namespace iChat.BackEnd.Services.Users.Infra.EFcore.MessageServices
{
    public class EfCoreMessageReadService : IMessageDbReadService
    {
        private readonly iChatDbContext _context;


        private static readonly Expression<Func<Message, ChatMessageDto>> _toDto = m => new ChatMessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            MessageType = m.MessageType,
            Content = m.TextContent ?? "",
            ContentMedia = m.MediaFile == null ? null :m.MediaFile.ToDto(),
            CreatedAt = m.Timestamp,
            IsEdited = m.LastEditedAt != null,
            IsDeleted = m.isDeleted,
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
                .Include(m => m.MediaFile)
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
                .Include(m => m.MediaFile)
                .Select(_toDto)
                .ToListAsync();

            var afterTask = _context.Messages
                .AsNoTracking()
                .Where(m => m.ChannelId == channelId && m.Id > messageId)
                .OrderBy(m => m.Id)
                .Take(AfterIndex)
                .Include(m => m.MediaFile)
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
            limit = Math.Min(limit, 250);
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

        public async Task<List<BucketDto>> GetLatestBucketsByChannelAsync(long channelId, int bucketCount = 3)
        {
            bucketCount = Math.Min(bucketCount, 5); 
            var results = await _context.Database
                .SqlQueryRaw<RawBucketResult>(
                    "SELECT * FROM get_latest_buckets_by_channel({0}, {1})",
                    channelId,
                    bucketCount)
                .ToListAsync();
            var buckets= results.Select(r => new BucketDto(r)).ToList();
            return buckets;
        }

        public async Task<List<BucketDto>> GetBucketsAroundMessageAsync(long channelId, long messageId, int bucketRange = 2)
        {
            bucketRange = Math.Min(bucketRange, 5); 
            var results = await _context.Database
                .SqlQueryRaw<RawBucketResult>($"SELECT * FROM get_buckets_around_message({channelId}, {messageId}, {bucketRange})")
                .ToListAsync();

            return results.Select(r => new BucketDto(r)).ToList();
        }
        public async Task<List<BucketDto>> GetBucketsInRangeAsync(long channelId, long startId, long endId, int limit = 50)
        {
            limit = Math.Min(limit, 250);
            var results = await _context.Database
                .SqlQueryRaw<RawBucketResult>($"SELECT * FROM get_buckets_by_bucket_id_range({channelId}, {startId}, {endId}, {limit})")
                .ToListAsync();

            return results.Select(r => new BucketDto(r)).ToList();
        }

        public async Task<BucketDto> GetBucketById(long channelId, int bucketId)
        {
            var result = await _context.Database.SqlQueryRaw<RawBucketResult>($"SELECT * FROM get_bucket_by_id({channelId},{bucketId})").FirstOrDefaultAsync();
            if (result==null)
            throw  new BadHttpRequestException($"Bucket with ID {bucketId} not found in channel {channelId}.");
            return new BucketDto( result);
        }
    }
}
