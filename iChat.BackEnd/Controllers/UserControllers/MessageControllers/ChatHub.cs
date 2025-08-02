
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

        public static string FocusServerKey(ServerId roomId) => $"{roomId}_focus";
		public static string FocusServerKey(long roomId) => $"{roomId}_focus";
		public static string FocusServerKey(string roomId) => $"{roomId}_focus";

		public static string FocusChannelKey(ChannelId channelId) => $"{channelId}c_focus";
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
            var userId = new UserClaimHelper(Context.User).GetUserIdSL();
            var connectionId = Context.ConnectionId;
            var serverList = _localCache.GetUserServerList(userId, true);
            if (serverList == null)
                throw new Exception("User hasnt cached");
            _connectionTracker.AddConnection(userId, connectionId);
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
            var userId = new UserClaimHelper(Context.User).GetUserIdSL();
            var connectionId = Context.ConnectionId;
            
            var finally_offline =_connectionTracker.RemoveConnection(userId, connectionId);
            if (finally_offline)
            {
               await _chatServerMetadataCacheService.SetUserOffline(userId);
            }
             await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> JoinRoom(ChatServerConnectionState state)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdSL();
            if (!await _chatServerMetadataCacheService.IsMember(state.serverId, state.channelId, userId))
                return false;
            var prevState = _connectionTracker.SetServer(Context.ConnectionId,state);
            if (prevState.serverId != state.serverId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, FocusServerKey(state.serverId));
                if (!prevState.serverId.Value.IsNull())
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusServerKey(prevState.serverId));

            }
            if(prevState.channelId!=state.channelId)
            {
                if (!prevState.channelId.Value.IsNull())
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusChannelKey(prevState.channelId));
                await Groups.AddToGroupAsync(Context.ConnectionId, FocusChannelKey(state.channelId));
            }

            await Clients.Caller.SendAsync(SignalrClientPath.UserList,await _chatServerMetadataCacheService.GetMemberList(state.serverId));
            _logger.LogInformation($"Client {Context.ConnectionId} joined room {state.serverId}");
            return true;
        }
        public async Task<bool> JoinChannel(string ChannelId)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdSL();
            var serverId = _connectionTracker.GetServer(Context.ConnectionId);
            var channelId = new ChannelId(new stringlong(ChannelId));

			if (serverId.Value.IsNull())
                return false;
            if (!await _chatServerMetadataCacheService.IsMember(serverId, channelId, userId))
                return false;
            var prev=_connectionTracker.SetChannel(Context.ConnectionId, channelId);
            if (!prev.Value.IsNull())
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusChannelKey(prev));
            await Groups.AddToGroupAsync(Context.ConnectionId, FocusChannelKey(channelId));
            return true;
        }
        public async Task LeaveRoom()
        {
            var prev = _connectionTracker.SetServer(Context.ConnectionId, new ChatServerConnectionState());
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusServerKey(prev.serverId));
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, FocusChannelKey(prev.channelId));

            _logger.LogInformation($"Client {Context.ConnectionId} left room {prev.serverId}");
        }
        public async Task SendMessage(string roomId, ChatMessageDtoSafe message)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            var isValid = _connectionTracker.ValidateConnection(new ServerId(new stringlong( roomId)),new ChannelId(new stringlong ( message.ChannelId)), Context.ConnectionId);
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
