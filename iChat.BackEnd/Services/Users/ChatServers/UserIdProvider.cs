using Azure.Core;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return new UserClaimHelper(connection.User).GetUserIdStr();
        }
    }
}
