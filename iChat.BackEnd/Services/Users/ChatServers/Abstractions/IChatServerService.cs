namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatServerService
    {
        Task<bool> CheckIfUserInServer(long userId, long serverId);
        Task<bool> CheckIfUserBanned(long userId, long serverId);
        Task Join(long userId, long serverId);
        Task BanUser(long userId, long serverId, long adminUserId);
        Task UnbanUser(long userId, long serverId, long adminUserId);
    }
}
