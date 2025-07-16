namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IUserPresenceCacheService
    {
        Task AddUserToServerAsync(long userId, long serverId);
        Task RemoveUserFromServerAsync(long userId, long serverId);
        Task SetUserOnlineAsync(long userId, long serverId);
        Task SetUserOfflineAsync(long userId, long serverId);
        Task<List<long>> GetOnlineUsersAsync(long serverId);
        Task<List<long>> GetOfflineUsersAsync(long serverId);
        Task<List<long>> GetSortedUserListAsync(long serverId);
        bool IsUserInServer(long userId, long serverId);
        bool IsUserOnlineInServer(long userId, long serverId);
    }

}
