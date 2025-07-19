using iChat.DTOs.Collections;
using iChat.DTOs.Users;


namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs
{
    public interface IUserConnectionTracker
    {
        bool AddConnection(stringlong userId, string connectionId);
        bool RemoveConnection(stringlong userId, string connectionId);
        IReadOnlyCollection<string> GetConnections(long userId);
        ChatServerConnectionState SetServer(string connectionId, ChatServerConnectionState state);
        stringlong GetServer(string connectionId);
        bool ValidateConnection(stringlong serverId, stringlong channelId, string connectionId);
        // Channel tracking
        stringlong SetChannel(string connectionId, stringlong channelId);
        stringlong GetChannelForConnection(string connectionId);
        IReadOnlyCollection<string> GetConnectionsInChannel(stringlong channelId);
    }
}

