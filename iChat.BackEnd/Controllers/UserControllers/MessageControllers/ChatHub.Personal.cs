using iChat.DTOs.Users.Enum;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    public partial class ChatHub : Hub
    {
        public async Task Typing()
        {
            var userId = new UserClaimHelper(Context.User).GetUserId();
            var channelId = _connectionTracker.GetChannelForConnection(Context.ConnectionId);
            Console.WriteLine($"{userId} is typing");
            if (channelId != null)
            {
                await Clients.Group(FocusChannelKey(channelId)).SendAsync(SignalrClientPath.UserTyping,  userId);
            }
        }
    }
}
