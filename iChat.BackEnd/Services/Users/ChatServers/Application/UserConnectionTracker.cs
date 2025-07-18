using iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class UserConnectionTracker : IUserConnectionTracker
    {
        private readonly ConcurrentDictionary<long, HashSet<string>> _connections = new();
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
            if (_connections.TryGetValue(userId, out var set))
            {
                lock (set)
                {
                    set.Remove(connectionId);
                    if (set.Count == 0)
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
    }
}
