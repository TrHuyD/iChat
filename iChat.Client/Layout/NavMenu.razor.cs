using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace iChat.Client.Layout
{
    public partial class NavMenu
    {
        private bool _showServerMenu = false;
        private int _contextMenuX = 0;
        private int _contextMenuY = 0;
        private ChatServerDtoUser _contextMenuServer = null;
        private List<UserMetadataReact> _filteredUsers = new List<UserMetadataReact>();
        private UserProfileDto userProfileDto = new UserProfileDto();
        private bool showCreateServer = false;
        private bool showCreateChannel = false;
        private ChatServerDtoUser? _selectedServer;
        private string _selectedChannelId= string.Empty;
        private string SelectedServerId => _selectedServer?.Id ?? string.Empty;
        protected override async Task OnInitializedAsync()
        {
            ChatNavService.ServerChanged += OnServerChange;
            ChatNavService.ChannelChanged += OnChannelChange;
            userProfileDto = LoadingService.GetUserProfile();
        }
        private void Reset()
        {
            _showServerMenu = false;
            _contextMenuX = 0;
            _contextMenuY = 0;
            _contextMenuServer = null;
            _filteredUsers = new List<UserMetadataReact>();
            showCreateServer = false;
            showCreateChannel = false;
            _selectedServer = null;

        }
        private void ShowServerContextMenu(MouseEventArgs e, ChatServerDtoUser server)
        {
            _contextMenuServer = server;
            _contextMenuX = (int)e.ClientX;
            _contextMenuY = (int)e.ClientY;
            _showServerMenu = true;
        }

        private void ShowServerDropdown(MouseEventArgs e)
        {
            if (_selectedServer != null)
            {
                _contextMenuServer = _selectedServer;
                _contextMenuX = (int)e.ClientX;
                _contextMenuY = (int)e.ClientY;
                _showServerMenu = true;
            }
        }

        private void HideContextMenu()
        {
            _showServerMenu = false;
            _contextMenuServer = null;
        }




        private async Task HandleLeaveServer()
        {
            Reset();
            Navigation.NavigateTo("/");
        }
        private async Task HandleInviteUser(string userId)
        {
        }

        private void ClearServerSelection()
        {
            _selectedServer = null;
            Navigation.NavigateTo("/");
            StateHasChanged();
        }

        public async Task ShowChannelList(string serverId)
        {
            var server = ChatNavService.GetServer(serverId);
            if (server == null) return;
            await ChatNavService.NavigateToServer(serverId, Navigation);
           
        }
        public void OnServerChange(ChatServerDtoUser server)
        {
            _selectedServer = server;
            StateHasChanged();
        }
        public void OnChannelChange(string channelId)
        {
            _selectedChannelId= channelId;
            StateHasChanged();
        }

        private async Task NavigateToChannel(string serverId, string channelId)
        {
            await ChatNavService.NavigateToChannel(serverId, channelId, Navigation);
        }
        public void Dispose()
        {
            LoadingService.OnAppReadyStateChanged -= StateHasChanged;
            ChatNavService.OnChatServersChanged -= StateHasChanged;
            ChatNavService.ServerChanged -= OnServerChange;
            ChatNavService.ChannelChanged -= OnChannelChange;

        }
    }
}
