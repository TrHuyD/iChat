
using Auth0.ManagementApi.Models;
using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Enum;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Data;
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

        public async Task JoinRoom(ChatServerConnectionState state)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            if (!await _chatServerMetadataCacheService.IsMember(state.ServerId, state.ChannelId, userId))
                return;
            var prevState = _connectionTracker.SetServer(Context.ConnectionId,state);
            if (prevState.ServerId != state.ServerId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, FocusServerKey(state.ServerId));
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusServerKey(prevState.ServerId));
            }
            await Clients.Caller.SendAsync(SignalrClientPath.UserList,await _chatServerMetadataCacheService.GetMemberList(state.ServerId));
            _logger.LogInformation($"Client {Context.ConnectionId} joined room {state.ServerId}");
        }
        public async Task JoinChannel(string ChannelId)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            var serverId = _connectionTracker.GetServer(Context.ConnectionId);
            if (serverId == 0)
                return;
            if (!await _chatServerMetadataCacheService.IsMember(serverId, ChannelId, userId))
                return;
            var prev=_connectionTracker.SetChannel(Context.ConnectionId,ChannelId);
            if (prev != 0)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusChannelKey(prev));
            await Groups.AddToGroupAsync(Context.ConnectionId, FocusChannelKey(ChannelId));
        }
        public async Task LeaveRoom()
        {
            var prev = _connectionTracker.SetServer(Context.ConnectionId, new ChatServerConnectionState(0,0));
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusServerKey(prev.ServerId));
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusChannelKey(prev.ChannelId));

            _logger.LogInformation($"Client {Context.ConnectionId} left room {prev.ServerId}");
        }
        public async Task SendMessage(string roomId, ChatMessageDtoSafe message)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            var isValid = _connectionTracker.ValidateConnection(roomId, message.ChannelId, Context.ConnectionId);
            if (!isValid)
                return;
            var request = new MessageRequest
            {
                SenderId = userId,
                TextContent = message.Content,
                ReceiveChannelId = message.ChannelId,
            };
            var result = await _sendMessageService.SendTextMessageAsync(request,roomId,false);
            _logger.LogInformation($"Message sent to room {roomId} by user {userId}");
            await Clients.Group(roomId).SendAsync(SignalrClientPath.RecieveMessage, result.Value);
        }

    }
}
