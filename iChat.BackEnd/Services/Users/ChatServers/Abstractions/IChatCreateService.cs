namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatCreateService
    {
        Task<long> CreateChannelAsync(long serverId, string channelName, long adminUserId);
        Task<string> CreateServerAsync(string serverName, long adminUserId);
    }
}
