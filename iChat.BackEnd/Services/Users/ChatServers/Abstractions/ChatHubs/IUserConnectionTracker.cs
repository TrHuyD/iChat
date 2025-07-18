using iChat.DTOs.Collections;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs
{
    public interface IUserConnectionTracker
    {
        bool AddConnection(long userId, string connectionId);
        bool RemoveConnection(long userId, string connectionId);
        IReadOnlyCollection<string> GetConnections(long userId);
        bool TrackRoom(string connectionId, stringlong roomName);
        stringlong GetRoom(string connectionId);

        // Channel tracking
        void SetChannel(string connectionId, string channelId);
        string? GetChannelForConnection(string connectionId);
        IReadOnlyCollection<string> GetConnectionsInChannel(string channelId);
    }
}

