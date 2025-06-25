using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace iChat.Client.Services.UserServices
{
    public class ChatNavigationService
    {
        public List<ChatServerDto> ChatServers { get; private set; } = new();

        public event Action OnChatServersChanged;
        public JwtAuthHandler _http;
        public ChatNavigationService(JwtAuthHandler jwtAuthHandler)
        {   
            _http = jwtAuthHandler ?? throw new ArgumentNullException(nameof(jwtAuthHandler));
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
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Server name must not be empty", nameof(name));
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Chat/Create")
            {
                Content = new StringContent(JsonSerializer.Serialize(new ChatServerCreateRq { Name = name }), Encoding.UTF8, "application/json")
            };
            var response = await _http.SendAuthAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create server: {errorContent}");
            }

            var newserver = await response.Content.ReadFromJsonAsync<ChatServerDto>();



            AddServer(newserver);
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