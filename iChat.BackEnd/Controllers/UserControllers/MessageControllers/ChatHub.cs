
using Auth0.ManagementApi.Models;
using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.DTOs.Collections;
using iChat.DTOs.Users.Enum;
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
    public partial class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IMessageWriteService _sendMessageService;
        private readonly MemCacheUserChatService _localCache; 

        public static string FocusServerKey(stringlong roomId) => $"{roomId}_focus";
        public static string FocusChannelKey(stringlong ChannelId) => $"{ChannelId}c_focus";
        private readonly AppChatServerCacheService _chatServerMetadataCacheService;

        private readonly IUserConnectionTracker _connectionTracker;
     
        public ChatHub(
            ILogger<ChatHub> logger,
            IMessageWriteService sendMessageService,
            MemCacheUserChatService memCacheUserChatService,
             IUserConnectionTracker connectionTracker,
          
          AppChatServerCacheService csmcs)
        {
            _chatServerMetadataCacheService = csmcs;
            _logger = logger;
            _sendMessageService = sendMessageService;
            _localCache = memCacheUserChatService;
            _connectionTracker = connectionTracker;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            var connectionId = Context.ConnectionId;
            var serverList = _localCache.GetUserServerList(userId, true);
            if (serverList == null)
                throw new Exception("User hasnt cached");
            _connectionTracker.AddConnection(long.Parse(userId), connectionId);
            foreach (var list in serverList)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, list.ToString());
            }
           // _presenceService.SetUserOnline(userId,serverList);
            _logger.LogInformation($"Client joining finished: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        public async Task HeartBeat()
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
           // var serverList = _localCache.GetUserServerList(userId, true);
            _localCache.RefreshOnlineState(userId);

         //   _presenceService.SetUserOnline(userId, serverList);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            var connectionId = Context.ConnectionId;
            var userIdLong = long.Parse(userId);
            var finally_offline =_connectionTracker.RemoveConnection(userIdLong, connectionId);
            if (finally_offline)
            {
               await _chatServerMetadataCacheService.SetUserOffline(userId);
            }
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
            
            await Groups.AddToGroupAsync(Context.ConnectionId, FocusServerKey(roomId));
            var prev =_connectionTracker.SetServer(roomId,userId);
            if(prev!=0)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusServerKey(prev));

            await Clients.Caller.SendAsync(SignalrClientPath.UserList,await _chatServerMetadataCacheService.GetMemberList(roomId));
            _logger.LogInformation($"Client {Context.ConnectionId} joined room {roomId}");
        }
        public async Task JoinChannel(string ChannelId)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            //Doesnt check for now
            var prev=_connectionTracker.SetChannel(Context.ConnectionId,ChannelId);
            if (prev != 0)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusChannelKey(prev));
            await Groups.AddToGroupAsync(Context.ConnectionId, FocusChannelKey(ChannelId));


        }
        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusServerKey(roomId));
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
            };
            var result = await _sendMessageService.SendTextMessageAsync(request,roomId);
            _logger.LogInformation($"Message sent to room {roomId} by user {userId}");
            await Clients.Group(roomId).SendAsync(SignalrClientPath.RecieveMessage, result.Value);
        }

    }
}
