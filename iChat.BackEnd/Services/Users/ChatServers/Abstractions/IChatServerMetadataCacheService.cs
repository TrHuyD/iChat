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
        Task<bool> UpdateServerMetadata(ChatServerChangeUpdate update);
        Task<OperationResultT<ChatServerMetadata>> GetServerAsync(ServerId serverId, bool isCopy = true);
        OperationResultBool IsAdmin(ServerId serverId, UserId userId);
        Task<OperationResultBool> IsAdmin(ServerId serverId, ChannelId channelId, UserId userId);
        (bool success, List<int> newOnlineLocations, List<int> oldOfflineLocations) SetUserOnline(List<long> serverIds, UserMetadata user);
        (bool success, List<long> serverList, List<int> newOfflineLocations, List<int> oldOnlineLocations) SetUserOffline(UserId userId);
        List<long> GetOnlineUsersAsync(ServerId serverId, int lim = 50);
        List<long> GetOfflineUsersAsync(ServerId serverId, int lim = 50);
        (int index, bool isOnline) AddUserToServer(UserId userId, ServerId serverId);
        bool RemoveUserFromServer(UserId userId, ServerId serverId);
        (List<long> online, List<long> offline) GetUserList(ServerId serverId);
    }
}
