using iChat.Client.Data.Chat;
using iChat.Client.Services.Auth;
using iChat.Client.Services.UI;
using iChat.Client.Services.UserServices.Chat.Util;
using iChat.DTOs.ChatServerDatas;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
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
        private readonly LastVisitedChannelService _channelTracker;
        public event Action<ChatServerDtoUser> ServerChanged;
        public event Action<string> ChannelChanged;
        private readonly Lazy<UserStateService> userStateService;

        NavigationManager nav;
        string CurrentServer = "";
        public ChatNavigationService(JwtAuthHandler jwtAuthHandler,ToastService toast, LastVisitedChannelService channelTracker, NavigationManager nav,
            Lazy< UserStateService> userStateService)
        {
            this.nav = nav;
            _http = jwtAuthHandler ?? throw new ArgumentNullException(nameof(jwtAuthHandler));
            _toastService = toast ?? throw new ArgumentNullException(nameof(toast));
            _channelTracker = channelTracker ;
            this.userStateService = userStateService;
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
            foreach (var i in chatServers)
                i.AvatarUrl = URLsanitizer.Apply(i.AvatarUrl);

            // Notify subscribers that the list has changed
            OnChatServersChanged?.Invoke();
        }
        public void AddChannel(ChatChannelDto ccdto)
        {
           var server= ChatServers.Where(s => s.Id == ccdto.ServerId.ToString()).FirstOrDefault();
            if (server == null)
                return;
            server.Channels.RemoveAll(c => c.Id == ccdto.Id);
            server.Channels.Add(ccdto);
            OnChatServersChanged?.Invoke();

        }
        public void UpdateServer(ChatServerChangeUpdate update)
        {
            var server = ChatServers.Where(s => s.Id == update.Id.ToString()).FirstOrDefault();
            if (server == null)
                return;


            if(update.Name!="")
                server.Name = update.Name;
            if(update.AvatarUrl!="")
                server.AvatarUrl=URLsanitizer.Apply(update.AvatarUrl);
            OnChatServersChanged?.Invoke();
        }
        public void RemoveServer(string serverId )
        {
            if (serverId == CurrentServer)
                nav.NavigateTo("/");
            var removed = ChatServers.RemoveAll(s => s.Id == serverId);
            if (removed > 0)
            {
                OnChatServersChanged?.Invoke();
            }
        }
        /// <summary>
        /// Adds a single server to the navigation
        /// </summary>
        public void AddServer(ChatServerData server)
        {
            int post = 0;
            if(ChatServers.Count!=0)
                post = ChatServers[^1].Position+1;
            AddServer(new ChatServerDtoUser
            {
                Id = server.Id.ToString(),
                Name = server.Name,
                AvatarUrl =URLsanitizer.Apply( server.AvatarUrl)    ,
                Channels = server.Channels,
                Position = post,
                isadmin=server.AdminId==userStateService.Value.GetUserProfile().userId
            });
            OnChatServersChanged?.Invoke();
        }
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
            OnChatServersChanged?.Invoke();
        }
        /// <summary>
        /// Gets a specific server by ID
        /// </summary>
        public ChatServerDtoUser GetServer(string serverId)
        {
            return ChatServers.FirstOrDefault(s => s.Id == serverId);
        }
     
        public async Task<bool> HasServer(string serverId)
        {
            if (string.IsNullOrEmpty(serverId))
            return false;
            return ChatServers.Any(s => s.Id == serverId);
        }
        public async Task NavigateToServer(string serverId)
        {
            var server = GetServer(serverId);
            if (server == null || server.Channels.Count == 0)
                return;

            var last = await _channelTracker.GetLastChannelAsync(serverId);
            var target = server.Channels.FirstOrDefault(c => c.Id == last)
                      ?? server.Channels.OrderBy(c => c.Order).FirstOrDefault();

            if (target != null)
            {
                await _channelTracker.SaveLastChannelAsync(serverId, target.Id);
                nav.NavigateTo($"/chat/{serverId}/{target.Id}");
            }
            ServerChanged?.Invoke(server);

        }
        public async Task NavigateToChannel(string serverId, string channelId)
        {
            
            await _channelTracker.SaveLastChannelAsync(serverId, channelId);
            nav.NavigateTo($"/chat/{serverId}/{channelId}");
        }
        public void  NavigateToHome()
        {
            nav.NavigateTo($"/");

        }
        public async Task OnServerChange(string serverId)
        {
            var server = GetServer(serverId);
            ServerChanged?.Invoke(server);
        }
        public async Task OnChannelChange(string channelId)
        {
            ChannelChanged?.Invoke(channelId);
        }
    }
}