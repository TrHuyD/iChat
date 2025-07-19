using iChat.DTOs.Collections;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB
{
    public interface IChatServerDbService
    {
        Task<bool> CheckIfUserInServer(stringlong userId, stringlong serverId);
        Task<bool> CheckIfUserBanned(stringlong userId, stringlong serverId);
        Task Join(stringlong userId, stringlong serverId);
        Task Left(stringlong userId, stringlong serverId);
        Task BanUserAsync(stringlong userId, stringlong serverId, stringlong adminUserId);
        Task UnbanUser(stringlong userId, stringlong serverId, stringlong adminUserId);
        Task TaskDeleteChatServerAsync(stringlong serverId, stringlong adminUserId);
        Task<ChatServerChangeUpdate> UpdateChatServerProfileAsync(stringlong serverId, stringlong adminUserId, string newName="", string url="");

    }
}
