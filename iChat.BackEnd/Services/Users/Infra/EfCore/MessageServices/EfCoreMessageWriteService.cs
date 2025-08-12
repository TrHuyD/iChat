using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Models.User;
using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.BackEnd.Services.Users.Infra.FileServices;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.Data.EF;
using iChat.Data.Entities.Logs;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;
using Microsoft.EntityFrameworkCore;
using System;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreMessageWriteService : IMessageDbWriteService
    {
        private readonly MessageWriteQueueService _queueService;

        iChatDbContext _context ;
        IGenericMediaUploadService _mediaUploadService ;
        public EfCoreMessageWriteService(MessageWriteQueueService queueService,
            iChatDbContext context,
            IGenericMediaUploadService uploadService)
        {
            _mediaUploadService = uploadService ;
            _context = context;
            _queueService = queueService;
        }

        public async Task<int> DeleteMessageAsync(DeleteMessageRq rq, bool hasAdminRight = false)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == rq.MessageId);
            if (message == null)
                throw new InvalidOperationException("Message not found");
            if (message.ChannelId != rq.ChannelId)
                throw new InvalidOperationException("Channel mismatch");
            if (message.isDeleted)
                throw new InvalidOperationException("Message already deleted");
            if (!hasAdminRight && message.SenderId != rq.UserId)
                throw new InvalidOperationException("Not authorized to delete this message");
            // Soft delete
            var tempo = "";
            if (message.MessageType == (short)MessageType.Media)
            {
                tempo = message.MediaId.ToString()??"";
                message.MediaId = null;
                message.MediaFile = null;

            }
            else
            {
                tempo=message.TextContent;
                message.TextContent = string.Empty;
            }
            message.isDeleted = true;
            message.LastEditedAt = DateTimeOffset.UtcNow;
            // Audit log
            var log = new MessageAuditLog
            {
                ChannelId = message.ChannelId,
                ChatChannel = message.ChatChannel,
                MessageId = message.Id,
                Message = message,
                ActionType = AuditActionType.Delete,
                Timestamp = DateTimeOffset.UtcNow,
                ActorUserId = rq.UserId,
                ActorUser = message.User,
                PreviousContent=tempo??""
            };
            _context.MessageAuditLogs.Add(log);
            await _context.SaveChangesAsync();
            return message.BucketId;
        }

        public async Task<int> EditMessageAsync(EditMessageRq rq)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == rq.MessageId);
            if (message == null)
                throw new InvalidOperationException("Message not found");
            if (message.ChannelId != rq.ChannelId)
                throw new InvalidOperationException("Channel mismatch");
            if (message.isDeleted)
                throw new InvalidOperationException("Cannot edit deleted message");
            if (message.SenderId != rq.UserId)
                throw new InvalidOperationException("Not authorized to edit this message");
            message.TextContent = rq.NewContent;
            message.LastEditedAt = DateTimeOffset.UtcNow;
            var tempo = "";
            if (message.MessageType == (short)MessageType.Media)
            {
                tempo = message.MediaId.ToString() ?? "";
                message.MediaId = null;
                message.MediaFile = null;

            }
            else
            {
                tempo = message.TextContent;
                message.TextContent = string.Empty;
            }
            var log = new MessageAuditLog
            {
                ChannelId = message.ChannelId,
                ChatChannel = message.ChatChannel,
                MessageId = message.Id,
                Message = message,
                ActionType = AuditActionType.Edit,
                Timestamp = DateTimeOffset.UtcNow,
                ActorUserId = rq.UserId,
                ActorUser = message.User,
                PreviousContent = tempo ?? ""

            };

            _context.MessageAuditLogs.Add(log);
            await _context.SaveChangesAsync();
            return message.BucketId;
        }
        public async Task<MediaFileDto> UploadImage(MessageUploadRequest request, SnowflakeIdDto messageId,long userId)
        {
            var result = (await _mediaUploadService.SaveImageAsync(request.File, new UserId( userId))).ToDto();
            _queueService.Enqueue(new MessageRequest
            {
                SenderId = userId.ToString(),
                ReceiveChannelId = request.ChannelId,
                MediaFileMetaData = result

            }, messageId);
            return result;
        }
        public async Task UploadMessageAsync(MessageRequest request, SnowflakeIdDto messageId)
        {
            if (string.IsNullOrEmpty(request.SenderId))
                throw new ArgumentException("SenderId is required.");

            _queueService.Enqueue(request, messageId);

        }

        public async Task UploadMessagesAsync(IEnumerable<(MessageRequest request, SnowflakeIdDto messageId)> messages)
        {
            foreach (var (req, id) in messages)
            {
                _queueService.Enqueue(req, id);
            }


        }
    }

}
