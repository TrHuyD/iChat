using iChat.BackEnd.Controllers.UserControllers.MessageControllers;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Enum;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public partial class ChatHubResponer
    {

        public async Task JoinNewServer(UserId userId, ServerId serverId)
        {

            var connections = _tracker.GetConnections(userId);
            foreach (var conn in connections)
            {
                await _chatHub.Groups.AddToGroupAsync(conn, serverId.ToString());
            }
            await _chatHub.Clients.User(userId.ToString()).SendAsync(SignalrClientPath.JoinNewServer, serverId);
        }

        public async Task LeaveServer(string userId, long serverId)
        {
            await _chatHub.Clients.User(userId.ToString()).SendAsync(SignalrClientPath.LeaverServer, serverId);
        }
        public async Task UpdateProfile(UserMetadata user)
        {
            await _chatHub.Clients.User(user.ToString()).SendAsync(SignalrClientPath.UpdateProfile, user);
        }

    }
}
