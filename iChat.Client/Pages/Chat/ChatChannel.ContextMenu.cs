using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        // Context menu fields
        private bool _showContextMenu = false;
        private string _contextMenuStyle = "";
        private string _contextMenuMessageId = "";
        private void ShowContextMenu(MouseEventArgs e, string messageId)
        {
            _contextMenuMessageId = messageId;
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
                _showContextMenu = false;
        }

        private async Task CopyMessageId()
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", _contextMenuMessageId);
            _showContextMenu = false;
        }
    }
}
