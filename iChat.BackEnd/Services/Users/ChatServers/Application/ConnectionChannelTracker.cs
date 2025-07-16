using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class ConnectionChannelTracker
    {
        private readonly ConcurrentDictionary<string, string> _connectionToChannel = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _channelToConnections = new();
        public void SetChannel(string connectionId, string channelId)
        {
            if (_connectionToChannel.TryGetValue(connectionId, out var previousChannel))
            {
                if (previousChannel == channelId)
                    return;
                if (_channelToConnections.TryGetValue(previousChannel, out var oldSet))
                {
                    lock (oldSet)
                    {
                        oldSet.Remove(connectionId);
                        if (oldSet.Count == 0)
                            _channelToConnections.TryRemove(previousChannel, out _);
                    }
                }
            }
            _connectionToChannel[connectionId] = channelId;

            _channelToConnections.AddOrUpdate(channelId,
                _ => new HashSet<string> { connectionId },
                (_, set) => { lock (set) { set.Add(connectionId); } return set; });
        }
        public void RemoveConnection(string connectionId)
        {
            if (_connectionToChannel.TryRemove(connectionId, out var channelId))
            {
                if (_channelToConnections.TryGetValue(channelId, out var set))
                {
                    lock (set)
                    {
                        set.Remove(connectionId);
                        if (set.Count == 0)
                            _channelToConnections.TryRemove(channelId, out _);
                    }
                }
            }
        }
        public string? GetChannelForConnection(string connectionId)
        {
            return _connectionToChannel.TryGetValue(connectionId, out var channel)
                ? channel
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