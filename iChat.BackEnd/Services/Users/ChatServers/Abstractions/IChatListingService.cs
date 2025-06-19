namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatListingService
    {
        Task<List<long>> GetServerChannelListAsync(long serverId);
        Task<List<long>> GetUserServersAsync(long userId);
        Task<List<long>> GetServerMembersAsync(long serverId);
        async Task<List<string>> GetServerChannelListAsStringAsync(long serverId)
    => (await GetServerChannelListAsync(serverId)).Select(id => id.ToString()).ToList();

        async Task<List<string>> GetUserServersAsStringAsync(long userId)
            => (await GetUserServersAsync(userId)).Select(id => id.ToString()).ToList();

        async Task<List<string>> GetServerMembersAsStringAsync(long serverId)
            => (await GetServerMembersAsync(serverId)).Select(id => id.ToString()).ToList();
    }
}
