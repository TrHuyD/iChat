﻿
using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
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

    //    private static readonly ConcurrentDictionary<string, string> UserFocusedChannel = new();
        public static string FocusKey(string roomId)=> $"{roomId}_focus";
        public static string FocusKey(long roomId) => $"{roomId}_focus";
        //    public static string PersonalKey(string userId) => $"{userId}_personal";
        public static string FocusChannelKey(string ChannelId) => $"{ChannelId}c_focus";
        private readonly AppChatServerCacheService _chatServerMetadataCacheService;

        private readonly IUserConnectionTracker _connectionTracker;
        private readonly ConnectionChannelTracker _connectionChannelTracker;
        public ChatHub(
            ILogger<ChatHub> logger,
            IMessageWriteService sendMessageService,
            MemCacheUserChatService memCacheUserChatService,
             IUserConnectionTracker connectionTracker,
           ConnectionChannelTracker connectionChannelTracker,
          AppChatServerCacheService csmcs)
        {
            _chatServerMetadataCacheService = csmcs;
            _connectionChannelTracker = connectionChannelTracker;
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
            _connectionChannelTracker.RemoveConnection(Context.ConnectionId);
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
        public async Task JoinChannel(string ChannelId)
        {
            var userId = new UserClaimHelper(Context.User).GetUserIdStr();
            //Doesnt check for now
            _connectionChannelTracker.SetChannel(Context.ConnectionId, ChannelId);
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
            };

            var result = await _sendMessageService.SendTextMessageAsync(request,roomId);
            _logger.LogInformation($"Message sent to room {roomId} by user {userId}");

            // Broadcast to all members in room
            await Clients.Group(roomId).SendAsync(SignalrClientPath.RecieveMessage, result.Value);
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
