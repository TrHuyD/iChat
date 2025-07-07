namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IMessageLastSeenService
    {
        /// <summary>
        /// Updates the last seen message for a user in a specific chat channel.
        /// </summary>
        Task UpdateLastSeenAsync(string chatChannelId,string serverId,string MessageId, string userId);
        /// <summary>
        /// Gets the last seen message for a user in a specific chat server.
        /// </summary>
        Task<Dictionary<string,string>> GetLastSeenMessageAsync(string serverId, string userId);
    }
}
