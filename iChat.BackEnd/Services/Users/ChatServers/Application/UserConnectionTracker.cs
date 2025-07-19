using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.DTOs.Collections;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class UserConnectionTracker : IUserConnectionTracker
    {
        private readonly ConcurrentDictionary<stringlong, HashSet<string>> _serverToConnections = new();
        private readonly ConcurrentDictionary<stringlong, HashSet<string>> _userToConnections = new();
        private readonly ConcurrentDictionary<string, (stringlong serverId, stringlong channelId)> _connectionToLoc = new();
        private readonly ConcurrentDictionary<stringlong, HashSet<string>> _channelToConnections = new();
        public bool AddConnection(stringlong userId, string connectionId)
        {
            bool alreadyExists = true;
            _userToConnections.AddOrUpdate(userId,
                _ =>
                {
                    alreadyExists = false;
                    return new HashSet<string> { connectionId };
                },
                (_, set) =>
                {
                    lock (set)
                    {
                        set.Add(connectionId);
                    }
                    return set;
                });

            return alreadyExists;
        }

        public bool RemoveConnection(stringlong userId, string connectionId)
        {
            if (_connectionToLoc.TryRemove(connectionId, out var loc))
            {
                if (loc.channelId != 0 && _channelToConnections.TryGetValue(loc.channelId, out var chanSet))
                {
                    lock (chanSet)
                    {
                        chanSet.Remove(connectionId);
                        if (chanSet.Count == 0)
                            _channelToConnections.TryRemove(loc.channelId, out _);
                    }
                }
                if (loc.serverId != 0 && _serverToConnections.TryGetValue(loc.serverId, out var serverSet))
                {
                    lock (serverSet)
                    {
                        serverSet.Remove(connectionId);
                        if (serverSet.Count == 0)
                            _serverToConnections.TryRemove(loc.serverId, out _);
                    }
                }
            }
            if (_userToConnections.TryGetValue(userId, out var userSet))
            {
                lock (userSet)
                {
                    userSet.Remove(connectionId);
                    if (userSet.Count == 0)
                    {
                        _userToConnections.TryRemove(userId, out _);
                        return true;
                    }
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyCollection<string> GetConnections(long userId)
        {
            return _userToConnections.TryGetValue(userId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }

        public stringlong SetServer(string connectionId, stringlong serverId)
        {
            var prevLoc = _connectionToLoc.GetOrAdd(connectionId, _ => (0, 0));
            if (prevLoc.serverId == serverId) return 0;
            if (prevLoc.serverId != 0 && _serverToConnections.TryGetValue(prevLoc.serverId, out var oldSet))
            {
                lock (oldSet)
                {
                    oldSet.Remove(connectionId);
                    if (oldSet.Count == 0)
                        _serverToConnections.TryRemove(prevLoc.serverId, out _);
                }
            }
            _serverToConnections.AddOrUpdate(serverId,
                _ => new HashSet<string> { connectionId },
                (_, set) => { lock (set) { set.Add(connectionId); } return set; });
            _connectionToLoc[connectionId] = (serverId, prevLoc.channelId);
            return prevLoc.serverId;
        }

        public stringlong GetServer(string connectionId)
        {
            return _connectionToLoc.TryGetValue(connectionId, out var loc)
                ? loc.serverId
                : 0;
        }

        public stringlong SetChannel(string connectionId, stringlong channelId)
        {
            var prevLoc = _connectionToLoc.GetOrAdd(connectionId, _ => (0, 0));
            if (prevLoc.channelId == channelId) return 0;
            if (prevLoc.channelId != 0 && _channelToConnections.TryGetValue(prevLoc.channelId, out var oldSet))
            {
                lock (oldSet)
                {
                    oldSet.Remove(connectionId);
                    if (oldSet.Count == 0)
                        _channelToConnections.TryRemove(prevLoc.channelId, out _);
                }
            }
            _channelToConnections.AddOrUpdate(channelId,
                _ => new HashSet<string> { connectionId },
                (_, set) => { lock (set) { set.Add(connectionId); } return set; });
            _connectionToLoc[connectionId] = (prevLoc.serverId, channelId);
            return prevLoc.channelId;
        }

        public stringlong GetChannelForConnection(string connectionId)
        {
            return _connectionToLoc.TryGetValue(connectionId, out var loc)
                ? loc.channelId
                : 0;
        }

        public IReadOnlyCollection<string> GetConnectionsInChannel(stringlong channelId)
        {
            return _channelToConnections.TryGetValue(channelId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }
    }
}
