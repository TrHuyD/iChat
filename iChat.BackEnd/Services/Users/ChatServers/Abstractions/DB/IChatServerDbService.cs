using iChat.DTOs.Collections;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB
{
    public interface IChatServerDbService
    {
        Task<bool> CheckIfUserInServer(UserId userId, ServerId serverId);
        Task<bool> CheckIfUserBanned(UserId userId, ServerId serverId);
        Task Join(UserId userId, ServerId serverId);
        Task Left(UserId userId, ServerId serverId);
        Task BanUserAsync(UserId userId, ServerId serverId, UserId adminUserId);
        Task UnbanUser(UserId userId, ServerId serverId, UserId adminUserId);
        Task TaskDeleteChatServerAsync(ServerId serverId, UserId adminUserId);
        Task<ChatServerChangeUpdate> UpdateChatServerProfileAsync(ServerId serverId, UserId adminUserId, string newName="", string url="");

    }
}
