using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class UserPresenceCacheService : IUserPresenceCacheService
    {
   
            private readonly ConcurrentDictionary<long, HashSet<long>> _serverUserMap = new();
            private readonly ConcurrentDictionary<long, HashSet<long>> _serverOnlineUserMap = new();

            public Task AddUserToServerAsync(long userId, long serverId)
            {
                var users = _serverUserMap.GetOrAdd(serverId, _ => new HashSet<long>());
                lock (users) users.Add(userId);
                return Task.CompletedTask;
            }

            public Task RemoveUserFromServerAsync(long userId, long serverId)
            {
                if (_serverUserMap.TryGetValue(serverId, out var users))
                    lock (users) users.Remove(userId);

                if (_serverOnlineUserMap.TryGetValue(serverId, out var onlineUsers))
                    lock (onlineUsers) onlineUsers.Remove(userId);

                return Task.CompletedTask;
            }

            public Task SetUserOnlineAsync(long userId, long serverId)
            {
                var online = _serverOnlineUserMap.GetOrAdd(serverId, _ => new HashSet<long>());
                lock (online) online.Add(userId);
                return Task.CompletedTask;
            }

            public Task SetUserOfflineAsync(long userId, long serverId)
            {
                if (_serverOnlineUserMap.TryGetValue(serverId, out var online))
                    lock (online) online.Remove(userId);
                return Task.CompletedTask;
            }

            public Task<List<long>> GetOnlineUsersAsync(long serverId)
            {
                if (!_serverUserMap.TryGetValue(serverId, out var allUsers) ||
                    !_serverOnlineUserMap.TryGetValue(serverId, out var onlineUsers))
                    return Task.FromResult(new List<long>());

                lock (allUsers)
                    lock (onlineUsers)
                        return Task.FromResult(allUsers.Intersect(onlineUsers).ToList());
            }

            public Task<List<long>> GetOfflineUsersAsync(long serverId)
            {
                if (!_serverUserMap.TryGetValue(serverId, out var allUsers))
                    return Task.FromResult(new List<long>());

                _serverOnlineUserMap.TryGetValue(serverId, out var onlineUsers);

                lock (allUsers)
                {
                    var offline = onlineUsers == null
                        ? allUsers.ToList()
                        : allUsers.Except(onlineUsers).ToList();
                    return Task.FromResult(offline);
                }
            }

            public Task<List<long>> GetSortedUserListAsync(long serverId)
            {
                if (!_serverUserMap.TryGetValue(serverId, out var allUsers))
                    return Task.FromResult(new List<long>());

                _serverOnlineUserMap.TryGetValue(serverId, out var onlineUsers);

                List<long> result;
                lock (allUsers)
                {
                    var online = (onlineUsers == null) ? new List<long>() : allUsers.Intersect(onlineUsers).ToList();
                    var offline = allUsers.Except(online).ToList();
                    result = new List<long>(online.Count + offline.Count);
                    result.AddRange(online);
                    result.AddRange(offline);
                }
                return Task.FromResult(result);
            }

            public bool IsUserInServer(long userId, long serverId)
            {
                return _serverUserMap.TryGetValue(serverId, out var users) && users.Contains(userId);
            }

            public bool IsUserOnlineInServer(long userId, long serverId)
            {
                return _serverOnlineUserMap.TryGetValue(serverId, out var online) && online.Contains(userId);
            }
        }

    }

