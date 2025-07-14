using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace iChat.Client.Modals.Servers
{
    public partial class ServerMenuModal
    {
        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
        [Parameter] public ChatServerDtoUser? Server { get; set; }
        [Parameter] public int ContextMenuX { get; set; }
        [Parameter] public int ContextMenuY { get; set; }
        [Parameter] public EventCallback OnLeaveServer { get; set; }
        [Parameter] public EventCallback<long> OnInviteUser { get; set; }
        [Inject] private UserMetadataService _userMetadataService { get; set; } = null!;

        private bool _inviteSectionVisible = false;
        private string _searchQuery = "";
        private List<UserMetadataReact> _filteredUsers = new List<UserMetadataReact>();
        private string _prevServerLink = "";
        private string _inviteLink = "";
        protected override void OnParametersSet()
        {
            if (IsVisible && Server != null)
            {
                UpdateFilteredUsers();
                if(_prevServerLink!=Server.Id)
                {
                    _inviteLink = string.Empty;
                    _prevServerLink = Server.Id;
                }
            }
        }
        private async Task HideContextMenu()
        {
            IsVisible = false;
            _inviteSectionVisible = false;
            await IsVisibleChanged.InvokeAsync(false);
        }

        private void ShowInviteSection()
        {
            _inviteSectionVisible = true;
            IsVisible = false;
            UpdateFilteredUsers();
            StateHasChanged();
        }

        private void UpdateFilteredUsers()
        {
            _filteredUsers = _userMetadataService.GetAll()
                .Where(u => u.DisplayName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private async Task LeaveServer()
        {
            await HideContextMenu();
            await OnLeaveServer.InvokeAsync();
        }

        private async Task InviteUser(long userId)
        {
            await OnInviteUser.InvokeAsync(userId);
        }

        private async Task GenerateInviteLink()
        {
            if (Server != null)
            {
                _inviteLink = await InviteService.CreateInvite(Server.Id);
                StateHasChanged();
            }
        }

        private async Task CopyToClipboard()
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", _inviteLink);
        }
    }
}
