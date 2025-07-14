using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB
{
    public interface IChatCreateDBService
    {
        Task<ChatChannelDto> CreateChannelAsync(long serverId, string channelName, long adminUserId);
        Task<ChatServerMetadata> CreateServerAsync(string serverName, long adminUserId);
    }
}
