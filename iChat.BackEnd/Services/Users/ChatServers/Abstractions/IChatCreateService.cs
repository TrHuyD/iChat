using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatCreateService
    {
        Task<ChatChannelDto> CreateChannelAsync(long serverId, string channelName, long adminUserId);
        Task<ChatServerDtoUser> CreateServerAsync(string serverName, long adminUserId);
    }
}
