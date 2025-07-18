using iChat.BackEnd.Controllers.UserControllers.MessageControllers;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Enum;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public partial class ChatHubResponer
    {

        public async Task JoinNewServer(string userId, ChatServerMetadata serverId)
        {
            var connections = _tracker.GetConnections(long.Parse(userId));
            foreach (var conn in connections)
            {
                await _chatHub.Groups.AddToGroupAsync(conn, serverId.Id);
            }
            await _chatHub.Clients.User(userId).SendAsync(SignalrClientPath.JoinNewServer, serverId);
        }

        public async Task LeaveServer(string userId, long serverId)
        {
            await _chatHub.Clients.User(userId).SendAsync(SignalrClientPath.LeaverServer, serverId);
        }
        public async Task UpdateProfile(UserMetadata user)
        {
            await _chatHub.Clients.User(user.UserId).SendAsync(SignalrClientPath.UpdateProfile, user);
        }

    }
}
