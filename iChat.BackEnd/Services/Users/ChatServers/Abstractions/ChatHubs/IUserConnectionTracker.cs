using iChat.DTOs.Collections;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs
{
    public interface IUserConnectionTracker
    {
        bool AddConnection(long userId, string connectionId);
        bool RemoveConnection(long userId, string connectionId);
        IReadOnlyCollection<string> GetConnections(long userId);
        stringlong SetServer(string connectionId, stringlong roomName);
        stringlong GetServer(string connectionId);

        // Channel tracking
        stringlong SetChannel(string connectionId, string channelId);
        stringlong GetChannelForConnection(string connectionId);
        IReadOnlyCollection<string> GetConnectionsInChannel(string channelId);
    }
}

