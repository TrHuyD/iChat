using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using iChat.DTOs.Collections;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class UserConnectionTracker : IUserConnectionTracker
    {
        private readonly ConcurrentDictionary<long, HashSet<string>> _connections = new();
        private readonly ConcurrentDictionary<string, stringlong> _connectionRooms = new();

        private readonly ConcurrentDictionary<string, stringlong> _connectionToChannel = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _channelToConnections = new();
        public bool AddConnection(long userId, string connectionId)
        {
            bool alreadyExists = true;
            _connections.AddOrUpdate(userId,
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
            _connectionRooms.TryRemove(connectionId, out _);
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

            if (_connections.TryGetValue(userId, out var userSet))
            {
                lock (userSet)
                {
                    userSet.Remove(connectionId);
                    if (userSet.Count == 0)
                    {
                        _connections.TryRemove(userId, out _);
                        return true;
                    }
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyCollection<string> GetConnections(long userId)
        {
            return _connections.TryGetValue(userId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }

        // --- Room Tracking ---
        public bool TrackRoom(string connectionId, stringlong roomName)
        {
            _connectionRooms[connectionId] = roomName;
            return true;
        }

        public stringlong GetRoom(string connectionId)
        {
            return _connectionRooms.TryGetValue(connectionId, out var room)
                ? room
                : default;
        }

        // --- Channel Tracking ---
        public void SetChannel(string connectionId, string channelId)
        {
            if (_connectionToChannel.TryGetValue(connectionId, out var previousChannel))
            {
                if (previousChannel.ToString() == channelId)
                    return;

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
        }

        public string? GetChannelForConnection(string connectionId)
        {
            return _connectionToChannel.TryGetValue(connectionId, out var channel)
                ? channel.ToString()
                : null;
        }

        public IReadOnlyCollection<string> GetConnectionsInChannel(string channelId)
        {
            return _channelToConnections.TryGetValue(channelId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }
    }
}
