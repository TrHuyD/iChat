namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatListingService
    {
        Task<List<long>> GetServerChannelListAsync(string serverId);
        Task<List<long>> GetUserServersAsync(string userId);
        Task<List<long>> GetServerMembersAsync(string serverId);
        async Task<List<string>> GetServerChannelListAsStringAsync(string serverId)
    => (await GetServerChannelListAsync(serverId)).Select(id => id.ToString()).ToList();

        async Task<List<string>> GetUserServersAsStringAsync(string userId)
            => (await GetUserServersAsync(userId)).Select(id => id.ToString()).ToList();

        async Task<List<string>> GetServerMembersAsStringAsync(string serverId)
            => (await GetServerMembersAsync(serverId)).Select(id => id.ToString()).ToList();
    }
}
