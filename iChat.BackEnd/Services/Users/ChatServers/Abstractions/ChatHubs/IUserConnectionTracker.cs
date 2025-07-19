using iChat.DTOs.Collections;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs
{
    public interface IUserConnectionTracker
    {
        bool AddConnection(stringlong userId, string connectionId);
        bool RemoveConnection(stringlong userId, string connectionId);
        IReadOnlyCollection<string> GetConnections(long userId);
        stringlong SetServer(string connectionId, stringlong roomName);
        stringlong GetServer(string connectionId);
        // Channel tracking
        stringlong SetChannel(string connectionId, stringlong channelId);
        stringlong GetChannelForConnection(string connectionId);
        IReadOnlyCollection<string> GetConnectionsInChannel(stringlong channelId);
    }
}

