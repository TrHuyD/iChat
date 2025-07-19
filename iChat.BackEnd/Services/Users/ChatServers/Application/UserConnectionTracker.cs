using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class UserConnectionTracker : IUserConnectionTracker
    {
        private readonly ConcurrentDictionary<stringlong, HashSet<string>> _serverToConnections = new();
        private readonly ConcurrentDictionary<stringlong, HashSet<string>> _userToConnections = new();
        private readonly ConcurrentDictionary<string, ChatServerConnectionState> _connectionToLoc = new();
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
                if (loc.ChannelId != 0 && _channelToConnections.TryGetValue(loc.ChannelId, out var chanSet))
                {
                    lock (chanSet)
                    {
                        chanSet.Remove(connectionId);
                        if (chanSet.Count == 0)
                            _channelToConnections.TryRemove(loc.ChannelId, out _);
                    }
                }
                if (loc.ServerId != 0 && _serverToConnections.TryGetValue(loc.ServerId, out var serverSet))
                {
                    lock (serverSet)
                    {
                        serverSet.Remove(connectionId);
                        if (serverSet.Count == 0)
                            _serverToConnections.TryRemove(loc.ServerId, out _);
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
        private readonly ConcurrentDictionary<string, object> _connectionLocks = new();

        public stringlong SetChannel(string connectionId, stringlong ChannelId)
        {
            var lockObj = _connectionLocks.GetOrAdd(connectionId, _ => new object());

            lock (lockObj)
            {
                var prevLoc = _connectionToLoc.GetOrAdd(connectionId, _ => new ChatServerConnectionState(0,0));
                if (prevLoc.ChannelId == ChannelId) return 0;

                if (prevLoc.ChannelId != 0 && _channelToConnections.TryGetValue(prevLoc.ChannelId, out var oldSet))
                {
                    lock (oldSet)
                    {
                        oldSet.Remove(connectionId);
                        if (oldSet.Count == 0)
                            _channelToConnections.TryRemove(prevLoc.ChannelId, out _);
                    }
                }

                _channelToConnections.AddOrUpdate(ChannelId,
                    _ => new HashSet<string> { connectionId },
                    (_, set) => { lock (set) { set.Add(connectionId); } return set; });

                _connectionToLoc[connectionId] = new ChatServerConnectionState(prevLoc.ServerId, ChannelId);
                return prevLoc.ChannelId;
            }
        }

        public ChatServerConnectionState SetServer(string connectionId, ChatServerConnectionState state)
        {
            var lockObj = _connectionLocks.GetOrAdd(connectionId, _ => new object());

            lock (lockObj)
            {
                var prevLoc = _connectionToLoc.GetOrAdd(connectionId, _ => new ChatServerConnectionState());
                if (prevLoc.ServerId == state.ServerId)
                    return prevLoc;
                if (prevLoc.ServerId != 0 && _serverToConnections.TryGetValue(prevLoc.ServerId, out var oldServerSet))
                {
                    lock (oldServerSet)
                    {
                        oldServerSet.Remove(connectionId);
                        if (oldServerSet.Count == 0)
                            _serverToConnections.TryRemove(prevLoc.ServerId, out _);
                    }
                }
                if (prevLoc.ChannelId != 0 && _channelToConnections.TryGetValue(prevLoc.ServerId, out var oldChannelSet))
                {
                    lock (oldChannelSet)
                    {
                        oldChannelSet.Remove(connectionId);
                        if (oldChannelSet.Count == 0)
                            _channelToConnections.TryRemove(prevLoc.ChannelId, out _);
                    }
                }
                _serverToConnections.AddOrUpdate(
                    state.ServerId,
                    _ => new HashSet<string> { connectionId },
                    (_, set) =>
                    {
                        lock (set) { set.Add(connectionId); }
                        return set;
                    });
                _channelToConnections.AddOrUpdate(state.ChannelId,
                _ => new HashSet<string> { connectionId },
                (_, set) => { lock (set) { set.Add(connectionId); } return set; });
                _connectionToLoc[connectionId] = state;

                return prevLoc;
            }
        }


        public stringlong GetServer(string connectionId)
        {
            return _connectionToLoc.TryGetValue(connectionId, out var loc)
                ? loc.ServerId
                : 0;
        }

       

        public stringlong GetChannelForConnection(string connectionId)
        {
            return _connectionToLoc.TryGetValue(connectionId, out var loc)
                ? loc.ChannelId
                : 0;
        }

        public IReadOnlyCollection<string> GetConnectionsInChannel(stringlong ChannelId)
        {
            return _channelToConnections.TryGetValue(ChannelId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }

        public bool ValidateConnection(stringlong ServerId, stringlong ChannelId, string connectionId)
        {
            var (cur_ServerId, cur_ChannelId) = _connectionToLoc[connectionId];
            return cur_ChannelId == ChannelId&&cur_ServerId==ServerId;
        }
    }
}
