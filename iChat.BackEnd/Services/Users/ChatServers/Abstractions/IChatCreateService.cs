using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatCreateService
    {
        Task<long> CreateChannelAsync(long serverId, string channelName, long adminUserId);
        Task<ChatServerDto> CreateServerAsync(string serverName, long adminUserId);
    }
}
