using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        // Context menu fields
        private bool _showContextMenu = false;
        private string _contextMenuStyle = "";
        private ChatMessageDtoSafe _contextMenuMessage =null;
        private void ShowContextMenu(MouseEventArgs e, ChatMessageDtoSafe messageId)
        {
            _contextMenuMessage = messageId;
            _contextMenuStyle = $"top: {e.ClientY}px; left: {e.ClientX}px;";
            _showContextMenu = true;
        }
        private void HideContextMenu()
        {
            _showContextMenu = false;
        }

        private void HandleEscapeKey(KeyboardEventArgs e)
        {
            if (e.Key == "Escape")
                HideContextMenu();
        }

        private async Task CopyMessageId()
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", _contextMenuMessage.Id);
            HideContextMenu();
        }
        private async Task DeleteMessage()
        {
            HideContextMenu();
            await    _messageHandleService.DeleteMessageAsync(new UserDeleteMessageRq
            {
                ChannelId = _currentChannel.Id,
                MessageId = _contextMenuMessage.Id,
                ServerId = _currentServer.Id,
            });
        }
    }
}
