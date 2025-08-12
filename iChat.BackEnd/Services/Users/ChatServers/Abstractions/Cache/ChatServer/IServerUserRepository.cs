using iChat.BackEnd.Models.ChatServer;
using iChat.DTOs.ChatServerDatas;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer
{
    public interface IServerUserRepository
    {
        (bool success, List<int> newOnlineLocations, List<int> oldOfflineLocations) SetUserOnline(List<long> serverIds, UserMetadata user);
        (bool success, List<long> serverList, List<int> newOfflineLocations, List<int> oldOnlineLocations) SetUserOffline(UserId userId);
        List<long> GetOnlineUsersAsync(ServerId serverId, int lim = 50);
        List<long> GetOfflineUsersAsync(ServerId serverId, int lim = 50);
        (int index, bool isOnline) AddUserToServer(UserId userId, ServerId serverId);
        bool RemoveUserFromServer(UserId userId, ServerId serverId);
        (List<long> online, List<long> offline) GetUserList(ServerId serverId);
        bool UploadServersAsync(List<ChatServerbulk> servers);
        bool UploadNewServerAsync(ChatServerData server);
    }
}
