using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.DTOs.Collections;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class UserConnectionTracker : IUserConnectionTracker
    {
        private readonly ConcurrentDictionary<stringlong, HashSet<string>> _serverToConnections = new();
        private readonly ConcurrentDictionary<string, stringlong> _connectionToServer = new();

        private readonly ConcurrentDictionary<string, stringlong> _connectionToChannel = new();
        private readonly ConcurrentDictionary<stringlong, HashSet<string>> _channelToConnections = new();
        public bool AddConnection(long userId, string connectionId)
        {
            bool alreadyExists = true;
            _serverToConnections.AddOrUpdate(userId,
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

        public bool RemoveConnection(long userId, string connectionId)
        {
            _connectionToServer.TryRemove(connectionId, out _);
            _connectionToChannel.TryRemove(connectionId, out var channelId);

            if (channelId != null && _channelToConnections.TryGetValue(channelId.ToString(), out var set))
            {
                lock (set)
                {
                    set.Remove(connectionId);
                    if (set.Count == 0)
                        _channelToConnections.TryRemove(channelId.ToString(), out _);
                }
            }

            if (_serverToConnections.TryGetValue(userId, out var userSet))
            {
                lock (userSet)
                {
                    userSet.Remove(connectionId);
                    if (userSet.Count == 0)
                    {
                        _serverToConnections.TryRemove(userId, out _);
                        return true;
                    }
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyCollection<string> GetConnections(long userId)
        {
            return _serverToConnections.TryGetValue(userId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }


        public stringlong SetServer(string connectionId, stringlong serverId)
        {
            stringlong prevServer = 0;
            if (_connectionToServer.TryGetValue(connectionId, out prevServer))
            {
                if (prevServer== serverId)
                    return 0;

                if (_serverToConnections.TryGetValue(prevServer, out var oldSet))
                {
                    lock (oldSet)
                    {
                        oldSet.Remove(connectionId);
                        if (oldSet.Count == 0)
                            _serverToConnections.TryRemove(prevServer.ToString(), out _);
                    }
                }
            }
            _connectionToServer[connectionId] = serverId;
            _serverToConnections.AddOrUpdate(serverId,
                _ => new HashSet<string> { connectionId },
                (_, set) => { lock (set) { set.Add(connectionId); } return set; });
            return prevServer;
        }

        public stringlong GetServer(string connectionId)
        {
            return _connectionToServer.TryGetValue(connectionId, out var room)
                ? room
                : default;
        }

        // --- Channel Tracking ---
        public stringlong SetChannel(string connectionId, string channelId)
        {
            stringlong previousChannel = 0;
            if (_connectionToChannel.TryGetValue(connectionId, out  previousChannel))
            {
                if (previousChannel.ToString() == channelId)
                    return 0;

                if (_channelToConnections.TryGetValue(previousChannel.ToString(), out var oldSet))
                {
                    lock (oldSet)
                    {
                        oldSet.Remove(connectionId);
                        if (oldSet.Count == 0)
                            _channelToConnections.TryRemove(previousChannel.ToString(), out _);
                    }
                }
            }
            _connectionToChannel[connectionId] = channelId;
            _channelToConnections.AddOrUpdate(channelId,
                _ => new HashSet<string> { connectionId },
                (_, set) => { lock (set) { set.Add(connectionId); } return set; });
            return previousChannel;
        }

        public stringlong GetChannelForConnection(string connectionId)
        {
            return _connectionToChannel.TryGetValue(connectionId, out var channel)
                ? channel
                : 0;
        }

        public IReadOnlyCollection<string> GetConnectionsInChannel(string channelId)
        {
            return _channelToConnections.TryGetValue(channelId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }
    }
}
