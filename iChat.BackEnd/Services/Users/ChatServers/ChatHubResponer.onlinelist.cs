using iChat.BackEnd.Controllers.UserControllers.MessageControllers;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Enum;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR;
using NetTopologySuite.Index.HPRtree;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public partial class ChatHubResponer
    {



        public async Task BroadcastUserOnline(stringlong userId, List<long> serverId)
        {
            await Task.WhenAll(serverId.Select(item => _chatHub.Clients.Group(ChatHub.FocusKey(item)).SendAsync(SignalrClientPath.NewUserOnline, userId)));
        }

   
        public async Task BroadcastUserOffline(stringlong userId, List<long> serverId)
        {
            await Task.WhenAll(serverId.Select(item => _chatHub.Clients.Group(ChatHub.FocusKey(item)).SendAsync(SignalrClientPath.NewUserOffline, userId)));
        }
        public async Task BroadcastNewUser(string userId, string serverId,bool isOnline)
        {
            await _chatHub.Clients.Group(ChatHub.FocusKey(ChatHub.FocusKey(serverId))).SendAsync(SignalrClientPath.NewComer, userId,isOnline);

        }
    }
}
