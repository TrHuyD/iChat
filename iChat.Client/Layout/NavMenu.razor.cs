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
        private string _inviteLink = "";
        private UserProfileDto userProfileDto = new UserProfileDto();
        private bool showCreateServer = false;
        private bool showCreateChannel = false;
        private ChatServerDtoUser? _selectedServer;
        private string SelectedServerId => _selectedServer?.Id ?? string.Empty;
        private void Reset()
        {
            _showServerMenu = false;
            _contextMenuX = 0;
            _contextMenuY = 0;
            _contextMenuServer = null;
            _filteredUsers = new List<UserMetadataReact>();
            _inviteLink = "";
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
            StateHasChanged();
        }
        private async Task ShowChannelList(ChatServerDtoUser server)
        {
            _selectedServer = server;
            var last = await ChannelTracker.GetLastChannelAsync(server.Id);
            var target = server.Channels.FirstOrDefault(c => c.Id == last) ??
                         server.Channels.OrderBy(c => c.Order).FirstOrDefault();
            if (target != null)
            {
                await ChannelTracker.SaveLastChannelAsync(server.Id, target.Id);
                Navigation.NavigateTo($"/chat/{server.Id}/{target.Id}");
            }
            StateHasChanged();
        }
        private async Task NavigateToChannel(string serverId, string channelId)
        {
            await ChannelTracker.SaveLastChannelAsync(serverId, channelId);
            Navigation.NavigateTo($"/chat/{serverId}/{channelId}");
        }
        public void Dispose()
        {
            LoadingService.OnAppReadyStateChanged -= StateHasChanged;
            ChatNavService.OnChatServersChanged -= StateHasChanged;
        }
    }
}
