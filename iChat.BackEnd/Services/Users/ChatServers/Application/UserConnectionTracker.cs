using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class UserConnectionTracker : IUserConnectionTracker
    {
        private readonly ConcurrentDictionary<long, HashSet<string>> _connections = new();
        public void AddConnection(long userId, string connectionId)
        {
            _connections.AddOrUpdate(userId,
                _ => new HashSet<string> { connectionId },
                (_, set) => { lock (set) { set.Add(connectionId); } return set; });
        }

        public void RemoveConnection(long userId, string connectionId)
        {
            if (_connections.TryGetValue(userId, out var set))
            {
                lock (set)
                {
                    set.Remove(connectionId);
                    if (set.Count == 0)
                        _connections.TryRemove(userId, out _);
                }
            }
        }
        public IReadOnlyCollection<string> GetConnections(long userId)
        {
            return _connections.TryGetValue(userId, out var set)
                ? set.ToList()
                : Array.Empty<string>();
        }
    }
}
