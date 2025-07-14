namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.ChatHubs
{
    public interface IUserConnectionTracker
    {
        void AddConnection(long userId, string connectionId);
        void RemoveConnection(long userId, string connectionId);
        IReadOnlyCollection<string> GetConnections(long userId);
    }
}
