using iChat.DTOs.Collections;
using iChat.DTOs.Users;


namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs
{
    public interface IUserConnectionTracker
    {
        bool AddConnection(UserId userId, string connectionId);
        bool RemoveConnection(UserId userId, string connectionId);
        IReadOnlyCollection<string> GetConnections(UserId userId);
        ChatServerConnectionState SetServer(string connectionId, ChatServerConnectionState state);
        ServerId GetServer(string connectionId);
        bool ValidateConnection(ServerId serverId, ChannelId channelId, string connectionId);
        // Channel tracking
        ChannelId SetChannel(string connectionId, ChannelId channelId);
        ChannelId GetChannelForConnection(string connectionId);
        IReadOnlyCollection<string> GetConnectionsInChannel(ChannelId channelId);
    }
}

