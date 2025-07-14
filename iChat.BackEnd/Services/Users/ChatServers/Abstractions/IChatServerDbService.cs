namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatServerDbService
    {
        Task<bool> CheckIfUserInServer(long userId, long serverId);
        Task<bool> CheckIfUserBanned(long userId, long serverId);
        Task Join(long userId, long serverId);
        Task Left(long userId, long serverId);
        Task BanUserAsync(long userId, long serverId, long adminUserId);
        Task UnbanUser(long userId, long serverId, long adminUserId);
        Task TaskDeleteChatServerAsync(long serverId, long adminUserId);
        Task UpdateChatServerNameAsync(long serverId, string newName, long adminUserId);
    }
}
