using iChat.DTOs.Users.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iChat.Client.Services.UserServices
{
    public class ChatNavigationService
    {
        public List<ChatServerDto> ChatServers { get; private set; } = new();

        public event Action OnChatServersChanged;

        public ChatNavigationService()
        {
        }

        /// <summary>
        /// Updates the navigation with the provided list of chat servers
        /// </summary>
        /// <param name="chatServers">List of chat servers to display in navigation</param>
        public void UpdateChatServers(List<ChatServerDto> chatServers)
        {
            if (chatServers == null)
                return;

            // Sort by position
            ChatServers = chatServers.OrderBy(x => x.Position).ToList();

            // Notify subscribers that the list has changed
            OnChatServersChanged?.Invoke();
        }

        /// <summary>
        /// Adds a single server to the navigation
        /// </summary>
        public void AddServer(ChatServerDto server)
        {
            if (server == null)
                return;

            // Remove if exists already (to avoid duplicates)
            ChatServers.RemoveAll(s => s.Id == server.Id);

            // Add the new server
            ChatServers.Add(server);

            // Re-sort
            ChatServers = ChatServers.OrderBy(x => x.Position).ToList();

            // Notify subscribers
            OnChatServersChanged?.Invoke();
        }

        /// <summary>
        /// Removes a server from navigation
        /// </summary>
        public void RemoveServer(string serverId)
        {
            if (string.IsNullOrEmpty(serverId))
                return;

            var removed = ChatServers.RemoveAll(s => s.Id == serverId);

            if (removed > 0)
            {
                OnChatServersChanged?.Invoke();
            }
        }

        /// <summary>
        /// Updates a server's details in the navigation
        /// </summary>
        public void UpdateServer(ChatServerDto updatedServer)
        {
            if (updatedServer == null || string.IsNullOrEmpty(updatedServer.Id))
                return;

            var existingServer = ChatServers.FirstOrDefault(s => s.Id == updatedServer.Id);
            if (existingServer != null)
            {
                // Replace the server
                var index = ChatServers.IndexOf(existingServer);
                ChatServers[index] = updatedServer;

                // Re-sort
                ChatServers = ChatServers.OrderBy(x => x.Position).ToList();

                OnChatServersChanged?.Invoke();
            }
        }

        /// <summary>
        /// Clears all servers from navigation
        /// </summary>
        public void ClearServers()
        {
            if (ChatServers.Count > 0)
            {
                ChatServers.Clear();
                OnChatServersChanged?.Invoke();
            }
        }
        public async Task CreateServerAsync(string name)
        {
            // Simulate server creation
            var newServer = new ChatServerDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                AvatarUrl = "https://cdn.discordapp.com/embed/avatars/0.png",
                Position = ChatServers.Count + 1
            };
            // Add the new server to the list
            AddServer(newServer);
        }
        /// <summary>
        /// Gets a specific server by ID
        /// </summary>
        public ChatServerDto GetServer(string serverId)
        {
            return ChatServers.FirstOrDefault(s => s.Id == serverId);
        }
    }
}