﻿using iChat.Client.DTOs.Chat;
using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;


namespace iChat.Client.Layout
{
    public partial class NavMenu
    {
        private bool _editServerModalVisible = false;




        private bool _showServerMenu = false;
        private int _contextMenuX = 0;
        private int _contextMenuY = 0;
        private ChatServerDtoUser _contextMenuServer = null;
        private List<UserMetadataReact> _filteredUsers = new List<UserMetadataReact>();
        private UserMetadataReact userProfileDto = new (0,"loading.....", "https://cdn.discordapp.com/embed/avatars/3.png",0);
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
            _userMetadataService.OnMetadataUpdated += () => StateHasChanged();
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
        private async Task HandleInviteUser(long userId)
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
            await ChatNavService.NavigateToServer(serverId);
           
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
            await ChatNavService.NavigateToChannel(serverId, channelId);
        }
        public void Dispose()
        {
            LoadingService.OnAppReadyStateChanged -= StateHasChanged;
            ChatNavService.OnChatServersChanged -= StateHasChanged;
            ChatNavService.ServerChanged -= OnServerChange;
            ChatNavService.ChannelChanged -= OnChannelChange;

        }
        private Task OnSave() => Task.CompletedTask;
        private async Task LogOut()
        {
            try
            {
#if DEBUG
                await JS.InvokeVoidAsync("logout");
#else
            await _https.SendAuthAsync(new HttpRequestMessage(HttpMethod.Post, "/api/auth/refreshtoken/logout"));
#endif
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Navigation.NavigateTo("/login",forceLoad:true);
        }
        private void OpenProfileModal()
        {
            profileChanger.ShowDefaultModal(OnSave);
        }
        private Task OnEditModalClose()
        {
            _editServerModalVisible = false;
            StateHasChanged();
            return Task.CompletedTask;
        }
        private Task HandleServerUpdated((string? newName, IBrowserFile? newAvatar) result)
        {
            ToastService.ShowSuccess("Server profile updated!");
            _editServerModalVisible = false;
            return Task.CompletedTask;
        }
        private void ShowEditServerModal()
        {
            _showServerMenu = false;
            _editServerModalVisible = true;
        }

    }
}
