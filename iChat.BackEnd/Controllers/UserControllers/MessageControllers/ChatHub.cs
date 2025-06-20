
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatSendMessageService _sendMessageService;
        private readonly IChatReadMessageService _readMessageService;
        private readonly MemCacheUserChatService _localCache; 

        private static readonly ConcurrentDictionary<string, string> UserFocusedChannel = new();

        public ChatHub(
            ILogger<ChatHub> logger,
            IChatSendMessageService sendMessageService,
            IChatReadMessageService readMessageService,
            MemCacheUserChatService memCacheUserChatService)
        {
            _logger = logger;
            _sendMessageService = sendMessageService;
            _readMessageService = readMessageService;
            _localCache = memCacheUserChatService;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            foreach(var list in _localCache.GetServerListAsync(new UserClaimHelper(Context.User).GetUserId()))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, list);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");


          //  UserFocusedChannel.TryRemove(Context.ConnectionId, out _);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            _logger.LogInformation($"Client {Context.ConnectionId} joined room {roomId}");
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            _logger.LogInformation($"Client {Context.ConnectionId} left room {roomId}");
        }

        public async Task FocusRoom(string roomId)
        {
            UserFocusedChannel[Context.ConnectionId] = roomId;
        }

        public async Task UnfocusRoom()
        {
            UserFocusedChannel.TryRemove(Context.ConnectionId, out _);
        }

        public async Task SendMessage(string roomId, ChatMessageDtoSafe message)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();

            var request = new MessageRequest
            {
                SenderId = userId,
                TextContent = message.Content,
                ReceiveChannelId = roomId,
                messageType = MessageType.Text
            };

            var result = await _sendMessageService.SendTextMessageAsync(request);
            _logger.LogInformation($"Message sent to room {roomId} by user {userId}");

            // Broadcast to all members in room
            await Clients.Group(roomId).SendAsync("ReceiveMessage", new ChatMessageDtoSafe(result.Value));
        }

        public async Task<List<ChatMessageDtoSafe>> GetMessageHistory(long roomId, long? beforeMessageId = null)
        {
            var request = new UserGetRecentMessageRequest
            {
                UserId = new UserClaimHelper(Context.User).GetUserId(),
                ChannelId = roomId,
                LastMessageId = beforeMessageId
            };

            var messages = await _readMessageService.RetrieveRecentMessage(request);
            List<ChatMessageDtoSafe> safeMessages = messages
                .Select(m => new ChatMessageDtoSafe(m))
                .ToList();
            return safeMessages;
        }
    }
}
