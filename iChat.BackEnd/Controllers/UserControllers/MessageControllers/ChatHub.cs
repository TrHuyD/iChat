
using iChat.BackEnd.Models.Helpers;
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
        private readonly IMessageWriteService _sendMessageService;
        private readonly MemCacheUserChatService _localCache; 

    //    private static readonly ConcurrentDictionary<string, string> UserFocusedChannel = new();
         static string FocusKey(string roomId)=> $"{roomId}_focus";
        private readonly IUserPresenceCacheService _presenceService;
        public ChatHub(
            ILogger<ChatHub> logger,
            IMessageWriteService sendMessageService,
            MemCacheUserChatService memCacheUserChatService,
             IUserPresenceCacheService presenceService
            )
        {
            _logger = logger;
            _sendMessageService = sendMessageService;
            _localCache = memCacheUserChatService;
            _presenceService = presenceService;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            var serverList = _localCache.GetServerListAsync(userId, true);
            foreach (var list in serverList)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, list.ToString());
            }
            _presenceService.SetUserOnline(userId,serverList);
            _logger.LogInformation($"Client joining finished: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        public async Task HeartBeat()
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            var serverList = _localCache.GetServerListAsync(userId, true);
            _presenceService.SetUserOnline(userId, serverList);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            var serverList = _localCache.GetServerListAsync(userId, true);
            _presenceService.SetUserOffline(userId, serverList);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            if(!ValueParser.TryLong(roomId, out var roomIdLong)&&roomIdLong<1_000_000_000l)
            {
                _logger.LogWarning($"Invalid roomId: {roomId} for user {userId}");
                return;
            }
            if (!_localCache.IsUserInServer(userId, roomIdLong))
            {
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, FocusKey(roomId));
            _logger.LogInformation($"Client {Context.ConnectionId} joined room {roomId}");
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusKey(roomId));
            _logger.LogInformation($"Client {Context.ConnectionId} left room {roomId}");
        }



        public async Task SendMessage(string roomId, ChatMessageDtoSafe message)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();

            var request = new MessageRequest
            {
                SenderId = userId,
                TextContent = message.Content,
                ReceiveChannelId = message.ChannelId,
                messageType = MessageType.Text
            };

            var result = await _sendMessageService.SendTextMessageAsync(request);
            _logger.LogInformation($"Message sent to room {roomId} by user {userId}");

            // Broadcast to all members in room
            await Clients.Group(roomId).SendAsync("ReceiveMessage", result.Value);
        }

        //public async Task<List<ChatMessageDtoSafe>> GetMessageHistory(string roomId, string? beforeMessageId = null)
        //{
        //    if(!ValueParser.TryLong(roomId, out var roomIdLong))
        //    {
        //        return new();
        //    }
        //    long? messageIdLong = null;
        //    if(beforeMessageId!=null)
        //    {
        //        if (ValueParser.TryLong(beforeMessageId, out var beforeMessageIdLong))
        //        messageIdLong = beforeMessageIdLong;
        //    }    
        //    var request = new UserGetRecentMessageRequest
        //    {
        //        UserId = new UserClaimHelper(Context.User).GetUserId(),
        //        ChannelId = roomIdLong,
        //        LastMessageId = messageIdLong
        //    };

        //    var messages = await _readMessageService.RetrieveRecentMessage(request);
        //    List<ChatMessageDtoSafe> safeMessages = messages
        //        .Select(m => new ChatMessageDtoSafe(m))
        //        .ToList();
        //    return safeMessages;
        //}
    }
}
