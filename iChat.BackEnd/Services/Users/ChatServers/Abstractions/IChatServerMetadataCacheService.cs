using iChat.BackEnd.Models.ChatServer;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatServerMetadataCacheService
    {
        Task<bool> UploadServersAsync(List<ChatServerbulk> servers);
        Task<bool> UploadNewServerAsync(ChatServerMetadata server);
        Task<bool> AddChannel(ChatChannelDto channel);
        Task<OperationResultT<ChatServerMetadata>> EditServerProfile(stringlong serverId, stringlong requestorId, string newName, string avatarUrl = "");
        Task<OperationResultT<ChatServerMetadata>> GetServerAsync(stringlong serverId, bool isCopy = true);
        OperationResultBool IsAdmin(stringlong serverId, stringlong userId);
        Task<OperationResultBool> IsAdmin(stringlong serverId, stringlong channelId, stringlong userId);
        (bool success, List<int> newOnlineLocations, List<int> oldOfflineLocations) SetUserOnline(List<long> serverIds, UserMetadata user);
        (bool success, List<long> serverList, List<int> newOfflineLocations, List<int> oldOnlineLocations) SetUserOffline(stringlong userId);
        List<long> GetOnlineUsersAsync(stringlong serverId, int lim = 50);
        List<long> GetOfflineUsersAsync(stringlong serverId, int lim = 50);
        (int index, bool isOnline) AddUserToServer(stringlong userId, stringlong serverId);
        bool RemoveUserFromServer(stringlong userId, stringlong serverId);
        (List<long> online, List<long> offline) GetUserList(stringlong serverId);
    }
}
