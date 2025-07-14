using iChat.BackEnd.Controllers.UserControllers.MessageControllers;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public partial class ChatHubResponer
    {
        public async Task MetadataUpdateAsync(string userId, UserMetadata metadata)
        {
            await _chatHub.Clients.Group(ChatHub.PersonalKey(userId)).SendAsync("UserMetadataUpdate", userId, metadata);
        }
        public async Task JoinNewServer(string userId, long serverId)
        {
            await _chatHub.Clients.Group(ChatHub.PersonalKey(userId)).SendAsync("JoinNewServer", serverId);
        }
        public async Task LeaveServer(string userId, long serverId)
        {
            await _chatHub.Clients.Group(ChatHub.PersonalKey(userId)).SendAsync("LeaveServer", serverId);
        }
    }
}
