
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.ViewModels.Users.Messages;
namespace iChat.BackEnd.Services.Users.ChatServers
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
        public async Task JoinRoom(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
   //         await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} joined {groupName}");
        }

        public async Task LeaveRoom(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
         //   await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} left {groupName}");
        }

        public async Task SendMessage(string roomId, ChatMessageDto message, [FromServices] IChatSendMessageService _writeService)
        {
            // Validate message placeholder
            //message.SenderId = new UserClaimHelper(Context.User).GetUserId();
            //message.CreatedAt =DateTimeOffset.Now;
            //message.RoomId = long.Parse(roomId);
            var request = new MessageRequest
            {
                SenderId = new UserClaimHelper(Context.User).GetUserIdStr(),
                TextContent = message.Content,
                ReceiveChannelId = roomId,
                // ContentMedia = message.ContentMedia,
                messageType = MessageType.Text
            };
            var result = await _writeService.SendTextMessageAsync(request);
            _logger.LogInformation($"Message sent to room {roomId} by {Context.UserIdentifier}");
            await Clients.Group(roomId).SendAsync("ReceiveMessage", result.Value);
        }
        public async Task<List<ChatMessageDto>> GetMessageHistory([FromServices] IChatReadMessageService _writeService,long roomId, long? beforeMessageId = null)
        {
            var rq = new UserGetRecentMessageRequest
            {
                UserId = new UserClaimHelper(Context.User).GetUserIdStr(),
                ChannelId = roomId.ToString(),
            };
            var message = await _writeService.RetrieveRecentMessage(rq);
            _logger.LogInformation($"Requesting message history for room {roomId} before message ID {beforeMessageId}");
            return message; 
        }
        

    }
}
