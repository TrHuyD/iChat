
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
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

        // Optional cache to store channel focus states (could be Redis-backed for scale)
        private static readonly ConcurrentDictionary<string, string> UserFocusedChannel = new();

        public ChatHub(
            ILogger<ChatHub> logger,
            IChatSendMessageService sendMessageService,
            IChatReadMessageService readMessageService)
        {
            _logger = logger;
            _sendMessageService = sendMessageService;
            _readMessageService = readMessageService;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");

            // Clean up any focused state
            UserFocusedChannel.TryRemove(Context.ConnectionId, out _);

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

        public async Task SendMessage(string roomId, ChatMessageDto message)
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
            await Clients.Group(roomId).SendAsync("ReceiveMessage", result.Value);

            // Optionally: push "notification" to unfocused users
            foreach (var connection in UserFocusedChannel.Where(kvp => kvp.Value != roomId))
            {
                await Clients.Client(connection.Key).SendAsync("NotifyMessage", roomId, result.Value);
            }
        }

        //public async Task<List<ChatMessageDto>> GetMessageHistory(long roomId, long? beforeMessageId = null)
        //{
        //    var request = new UserGetRecentMessageRequest
        //    {
        //        UserId = new UserClaimHelper(Context.User).GetUserIdStr(),
        //        ChannelId = roomId.ToString(),
        //        LastMessageId = beforeMessageId
        //    };

        //    var messages = await _readMessageService.RetrieveRecentMessage(request);
        //    return messages;
        //}
    }
}
