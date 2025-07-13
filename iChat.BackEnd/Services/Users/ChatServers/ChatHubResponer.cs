using iChat.BackEnd.Controllers.UserControllers.MessageControllers;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public class ChatHubResponer
    {
        readonly IHubContext<ChatHub> _chatHub;
        public ChatHubResponer(IHubContext<ChatHub> chatHub)
        {
            _chatHub = chatHub;
        }
        public async Task EditedMessage( EditMessageRt rt)
        {
            await _chatHub.Clients.Group(rt.ServerId).SendAsync("MessageEdit", rt);
        }
        public async Task DeletedMessage(DeleteMessageRt rt)
        {
            await _chatHub.Clients.Group(rt.ServerId).SendAsync("MessageDelete", rt);
        }
    }
}
