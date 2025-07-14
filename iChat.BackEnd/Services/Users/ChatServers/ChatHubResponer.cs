using iChat.BackEnd.Controllers.UserControllers.MessageControllers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public partial class ChatHubResponer
    {
        readonly IHubContext<ChatHub> _chatHub;
        readonly private IUserConnectionTracker _tracker;
        public ChatHubResponer(IHubContext<ChatHub> chatHub,IUserConnectionTracker tracker)
        {
            _chatHub = chatHub;
            _tracker = tracker;
        }
        public async Task NewChannel(string ServerId,ChatChannelDto dto)
        {
            await _chatHub.Clients.Groups(ServerId).SendAsync("ChannelCreate", dto);

        }

        public async Task EditedMessage(EditMessageRt rt)
        {
            await _chatHub.Clients.Group(rt.ServerId).SendAsync("MessageEdit", rt);
        }
        public async Task DeletedMessage(DeleteMessageRt rt)
        {
            await _chatHub.Clients.Group(rt.ServerId).SendAsync("MessageDelete", rt);
        }
    }
}
