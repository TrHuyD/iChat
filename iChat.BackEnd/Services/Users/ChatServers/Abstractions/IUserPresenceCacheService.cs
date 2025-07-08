namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IUserPresenceCacheService
    {
        public void SetUserOnline(string userId, IEnumerable<long> serverIds);

        public List<string> GetOnlineUsersPaged(long serverId, int page = 0, int pageSize = 50);
        public void SetUserOffline(string userId, IEnumerable<long> serverIds);
        public void RemoveUserFromServer(string userId, long serverId);

    }
}
