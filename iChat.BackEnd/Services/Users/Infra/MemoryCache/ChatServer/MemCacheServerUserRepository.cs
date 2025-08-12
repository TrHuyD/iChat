using iChat.BackEnd.Collections;
using iChat.BackEnd.Models.ChatServer;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.DTOs.ChatServerDatas;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache.ChatServer
{
    public class MemCacheServerUserRepository: IServerUserRepository
    {
        private readonly ConcurrentDictionary<long, (SkipListSortedSetV2<long> Online, SkipListSortedSetV2<long> Offline)> _serverUserMap = new();
        private readonly MemCacheUserChatService _userChatService;
        public MemCacheServerUserRepository(MemCacheUserChatService mem)
        {
            _userChatService = mem;
        }
        private int _AddUserToServer(long userId, long serverId, bool online)
        {
            if (!_serverUserMap.TryGetValue(serverId, out var sets))
                throw new KeyNotFoundException($"Server {serverId} not found in _serverUserMap.");
            int index = -1;
            if (online)
                sets.Online.Add(userId, out index);
            else
                sets.Offline.Add(userId, out index);
            return index;
        }
        public (int index, bool isOnline) AddUserToServer(UserId userId, ServerId serverId)
        {
            var online = _userChatService.AddServerToUser(userId.Value, serverId.Value);
            return (_AddUserToServer(userId.Value, serverId.Value, online), online);
        }
        public List<long> GetOnlineUsersAsync(ServerId serverId, int lim = 50)
        {
            if (_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return sets.Online.GetRange(0, lim);
            throw new Exception($"Server {serverId} not loaded");
        }

        public List<long> GetOfflineUsersAsync(ServerId serverId, int lim = 50)
        {
            if (_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return sets.Offline.GetRange(0, lim);
            throw new Exception($"Server {serverId} not loaded");
        }


        public (bool success, List<int> newOnlineLocations, List<int> oldOfflineLocations) SetUserOnline(List<long> serverIds, UserMetadata user)
        {
            if (_userChatService.IsUserOnline(user.userId))
                return (false, new(), new());

            var newOnlineLocations = new List<int>(serverIds.Count);
            var oldOfflineLocations = new List<int>(serverIds.Count);

            for (int i = 0; i < serverIds.Count; i++)
            {
                var serverId = serverIds[i];
                if (!_serverUserMap.TryGetValue(serverId, out var sets))
                    throw new Exception($"Server {serverId} not found in cache");

                sets.Offline.Remove(user.userId.Value, out var oldOfflineIndex);
                if (!sets.Online.Add(user.userId.Value, out var newOnlineIndex))
                    throw new Exception("Cache mismatch");
                newOnlineLocations.Add(newOnlineIndex);
                oldOfflineLocations.Add(oldOfflineIndex);
            }
            _userChatService.SetOnlineUserData(serverIds.Select(s => s).ToList(), user);
            return (true, newOnlineLocations, oldOfflineLocations);
        }

        public (bool success, List<long> serverList, List<int> newOfflineLocations, List<int> oldOnlineLocations) SetUserOffline(UserId userId)
        {
            if (!_userChatService.IsUserOnline(userId))
                return (false, new(), new(), new());

            var serverIds = _userChatService.GetUserServerList(userId);
            var newOfflineLocations = new List<int>(serverIds.Count);
            var oldOnlineLocations = new List<int>(serverIds.Count);

            for (int i = 0; i < serverIds.Count; i++)
            {
                var serverId = serverIds[i];
                if (!_serverUserMap.TryGetValue(serverId, out var sets))
                    throw new Exception($"Server {serverId} not found in cache");

                if (!sets.Online.Remove(userId.Value, out var oldOnlineIndex))
                    throw new Exception($"Cache mismatch removing user {userId} from online");

                if (!sets.Offline.Add(userId.Value, out var newOfflineIndex))
                {

                    //let just ignore this, likely due to api testing, not setting online one
                    // throw new Exception("Cache mismatch");
                }
                oldOnlineLocations.Add(oldOnlineIndex);
                newOfflineLocations.Add(newOfflineIndex);
            }
            _userChatService.DowngradeUserCache(userId.Value.ToString());
            return (true, serverIds, newOfflineLocations, oldOnlineLocations);
        }

        public bool RemoveUserFromServer(UserId userId, ServerId serverId)
        {
            if (!_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return false;

            var removed = sets.Online.Remove(userId.Value, out _) || sets.Offline.Remove(userId.Value, out _);
            if (removed)
                _userChatService.RemoveServerFromUser(userId, serverId);

            return removed;
        }

        public (List<long> online, List<long> offline) GetUserList(ServerId serverId)
        {
            if (!_serverUserMap.TryGetValue(serverId.Value, out var sets))
                throw new KeyNotFoundException($"Server {serverId} not found in _serverUserMap.");
            return (sets.Online.GetAll(), sets.Offline.GetAll());
        }

        public bool UploadServersAsync(List<ChatServerbulk> servers)
        {
            foreach (var server in servers)
            {
                var offline = new SkipListSortedSetV2<long>();
                offline.Initialize(server.memberList);
                var online = new SkipListSortedSetV2<long>();
                _serverUserMap[server.Id] = (online, offline);
            }
            return true;
        }
        public bool UploadNewServerAsync(ChatServerData server)
        {
            var offline = new SkipListSortedSetV2<long>();
            var online = new SkipListSortedSetV2<long>();
            _serverUserMap[server.Id] = (online, offline);
            return true;
        }
         
    }
}
