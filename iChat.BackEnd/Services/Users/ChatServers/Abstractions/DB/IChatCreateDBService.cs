using iChat.DTOs.ChatServerDatas;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB
{
    public interface IChatCreateDBService
    {
        Task<ChatChannelDto> CreateChannelAsync(long serverId, string channelName, long adminUserId);
        Task<ChatServerData> CreateServerAsync(string serverName, long adminUserId);
    }
}
