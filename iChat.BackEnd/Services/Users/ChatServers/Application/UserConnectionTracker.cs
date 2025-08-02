using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class UserConnectionTracker : IUserConnectionTracker
    {
        private readonly ConcurrentDictionary<ServerId, HashSet<string>> _serverToConnections = new();
        private readonly ConcurrentDictionary<UserId, HashSet<string>> _userToConnections = new();
        private readonly ConcurrentDictionary<string, ChatServerConnectionState> _connectionToLoc = new();
        private readonly ConcurrentDictionary<ChannelId, HashSet<string>> _channelToConnections = new();
        public bool AddConnection(UserId userId, string connectionId)
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

        public bool RemoveConnection(UserId userId, string connectionId)
        {
            if (_connectionToLoc.TryRemove(connectionId, out var loc))
            {
                if (loc.channelId.Value is not null && _channelToConnections.TryGetValue(loc.channelId, out var chanSet))
                {
                    lock (chanSet)
                    {
                        chanSet.Remove(connectionId);
                        if (chanSet.Count == 0)
                            _channelToConnections.TryRemove(loc.channelId, out _);
                    }
                }
                if (loc.serverId.Value is not null && _serverToConnections.TryGetValue(loc.serverId, out var serverSet))
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

        public IReadOnlyCollection<string> GetConnections(UserId userId)
        {
            return _userToConnections.TryGetValue(userId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }
        private readonly ConcurrentDictionary<string, object> _connectionLocks = new();

        public ChannelId SetChannel(string connectionId, ChannelId channelId)
        {
            var lockObj = _connectionLocks.GetOrAdd(connectionId, _ => new object());

            lock (lockObj)
            {
                var prevLoc = _connectionToLoc.GetOrAdd(connectionId, _ => new ChatServerConnectionState());
                if (prevLoc.channelId == channelId) return new();

                if (prevLoc.channelId.Value is not null && _channelToConnections.TryGetValue(prevLoc.channelId, out var oldSet))
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

                _connectionToLoc[connectionId] = new ChatServerConnectionState(prevLoc.serverId, channelId);
                return prevLoc.channelId;
            }
        }

        public ChatServerConnectionState SetServer(string connectionId, ChatServerConnectionState state)
        {
            var lockObj = _connectionLocks.GetOrAdd(connectionId, _ => new object());

            lock (lockObj)
            {
                var prevLoc = _connectionToLoc.GetOrAdd(connectionId, _ => new ChatServerConnectionState());
                if (prevLoc.serverId == state.serverId)
                    return prevLoc;
                if (prevLoc.serverId.Value is not null && _serverToConnections.TryGetValue(prevLoc.serverId, out var oldServerSet))
                {
                    lock (oldServerSet)
                    {
                        oldServerSet.Remove(connectionId);
                        if (oldServerSet.Count == 0)
                            _serverToConnections.TryRemove(prevLoc.serverId, out _);
                    }
                }
                if (prevLoc.channelId.Value is not null && _channelToConnections.TryGetValue(prevLoc.channelId, out var oldChannelSet))
                {
                    lock (oldChannelSet)
                    {
                        oldChannelSet.Remove(connectionId);
                        if (oldChannelSet.Count == 0)
                            _channelToConnections.TryRemove(prevLoc.channelId, out _);
                    }
                }
                _serverToConnections.AddOrUpdate(
                    state.serverId,
                    _ => new HashSet<string> { connectionId },
                    (_, set) =>
                    {
                        lock (set) { set.Add(connectionId); }
                        return set;
                    });
                _channelToConnections.AddOrUpdate(state.channelId,
                _ => new HashSet<string> { connectionId },
                (_, set) => { lock (set) { set.Add(connectionId); } return set; });
                _connectionToLoc[connectionId] = state;

                return prevLoc;
            }
        }


        public ServerId GetServer(string connectionId)
        {
            return _connectionToLoc.TryGetValue(connectionId, out var loc)
                ? loc.serverId
                : new ServerId();
        }

       

        public ChannelId GetChannelForConnection(string connectionId)
        {
            return _connectionToLoc.TryGetValue(connectionId, out var loc)
                ? loc.channelId
                : new();
        }

        public IReadOnlyCollection<string> GetConnectionsInChannel(ChannelId channelId)
        {
            return _channelToConnections.TryGetValue(channelId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }

        public bool ValidateConnection(ServerId serverId, ChannelId channelId, string connectionId)
        {
            var (cur_ServerId, cur_ChannelId) = _connectionToLoc[connectionId];
            return cur_ChannelId == channelId && cur_ServerId==serverId;
        }
    }
}
