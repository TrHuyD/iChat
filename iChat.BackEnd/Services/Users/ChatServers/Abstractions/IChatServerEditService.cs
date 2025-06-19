namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IChatServerEditService
    {
        Task<bool> UpdateChatServerNameAsync(string serverId, string newName, string adminUserId);
        Task<bool> DeleteChatServerAsync(string serverId, string adminUserId);
    }
}
