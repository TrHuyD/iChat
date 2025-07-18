namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs
{
    public interface IUserConnectionTracker
    {
        bool AddConnection(long userId, string connectionId);
        bool RemoveConnection(long userId, string connectionId);
        IReadOnlyCollection<string> GetConnections(long userId);
    }
}
