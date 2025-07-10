using iChat.Client.Services.Auth;
using iChat.Client.Services.UI;
using iChat.DTOs.Users.Messages;
using System.Collections.Concurrent;
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
        public List<ChatServerDtoUser> ChatServers { get; private set; } = new();

        public event Action OnChatServersChanged;
        public JwtAuthHandler _http;
        private readonly ToastService _toastService;

        public ChatNavigationService(JwtAuthHandler jwtAuthHandler,ToastService toast)
        {   
            _http = jwtAuthHandler ?? throw new ArgumentNullException(nameof(jwtAuthHandler));
            _toastService = toast ?? throw new ArgumentNullException(nameof(toast));
        }

        /// <summary>
        /// Updates the navigation with the provided list of chat servers
        /// </summary>
        /// <param name="chatServers">List of chat servers to display in navigation</param>
        public void UpdateChatServers(List<ChatServerDtoUser> chatServers)
        {
            if (chatServers == null)
                return;

            // Sort by position
            ChatServers = chatServers.OrderBy(x => x.Position).ToList();

            // Notify subscribers that the list has changed
            OnChatServersChanged?.Invoke();
        }
        public void AddChannel(ChatChannelDto ccdto)
        {
           var server= ChatServers.Where(s => s.Id == ccdto.ServerId).FirstOrDefault();
            if (server == null)
                return;
            server.Channels.RemoveAll(c => c.Id == ccdto.Id);
            server.Channels.Add(ccdto);

        }
        /// <summary>
        /// Adds a single server to the navigation
        /// </summary>
        public void AddServer(ChatServerDtoUser server)
        {
            if (server == null)
                return;
            ChatServers.RemoveAll(s => s.Id == server.Id);
            ChatServers.Add(server);
            ChatServers = ChatServers.OrderBy(x => x.Position).ToList();
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
        public void UpdateServer(ChatServerDtoUser updatedServer)
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
        public async Task CreateChannelAsync(string name,string serverId)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Channel name must not be empty", nameof(name));
            if (string.IsNullOrWhiteSpace(serverId))
                throw new ArgumentException("Server ID must not be empty", nameof(serverId));
            var request= new HttpRequestMessage(HttpMethod.Post, "/api/Chat/CreateChannel")
            {
                Content = new StringContent(JsonSerializer.Serialize(new ChatChannelCreateRq { Name = name, ServerId = serverId }), Encoding.UTF8, "application/json")
            };
            var response = await _http.SendAuthAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _toastService.ShowToast($"Failed to create channel: {errorContent}","danger");
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

            var newserver = await response.Content.ReadFromJsonAsync<ChatServerDtoUser>();

            AddServer(newserver);
        }
        /// <summary>
        /// Gets a specific server by ID
        /// </summary>
        public ChatServerDtoUser GetServer(string serverId)
        {
            return ChatServers.FirstOrDefault(s => s.Id == serverId);
        }
        Dictionary<string,string> CachedInviteLink= new Dictionary<string, string>();
        public async Task<string> GetInviteLink(string serverId)
        {
            if(CachedInviteLink.TryGetValue(serverId, out var cachedLink))
            {
                return cachedLink;
            }
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Chat/{serverId}/InviteLink");
            var response = await _http.SendAuthAsync(request);
            return "";
        }
    }
}