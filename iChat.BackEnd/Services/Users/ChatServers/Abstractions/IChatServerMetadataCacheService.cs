using iChat.BackEnd.Models.ChatServer;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatServerMetadataCacheService
    {
        Task<bool> UploadServersAsync(List<ChatServerbulk> servers);
        void UploadServerAsync(ChatServerMetadata server);
        void AddChannelAsync(ChatChannelDto server);
        Task<ChatServerMetadata?> GetServerAsync(string serverId, bool includeChannels = true);
        Task<long?> GetUserPermissionAsync(long userId, long serverId, long channelId);
        Task<bool> SetUserPermissionAsync(long userId, long serverId, long channelId, long perm);
        Task<bool> IsAdmin(long serverId, long userId);
        Task<bool> IsAdmin(long serverId, long channelId,long userId);
        Task IsInServerWithCorrectStruct(long userId, long serverId, long channelId);
        Task<Dictionary<long, long>> GetAllUserPermsInServerAsync(long userId, long serverId);
        Task<Dictionary<long, long>> GetAllUserPermsInChannelAsync(long serverId, long channelId);
        bool SetUserOnline(List<long> serverList, UserMetadata metadata, long userId = -1);
        void AddUserToServer(long userId, long serverId, bool  online);
        bool RemoveUserFromServer(long userId, long serverId);

    }
}
