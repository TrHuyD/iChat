using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    public partial class ChatHub : Hub
    {
        public async Task Typing()
        {
            var userId = new UserClaimHelper(Context.User).GetUserId();
            var channelId = _connectionChannelTracker.GetChannelForConnection(Context.ConnectionId);
            Console.WriteLine($"{userId} is typing");
            if (channelId != null)
            {
                await Clients.Group(FocusChannelKey(channelId)).SendAsync("UserTyping", channelId, userId);
            }
        }
    }
}
