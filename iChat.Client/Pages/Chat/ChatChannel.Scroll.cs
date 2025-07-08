using Microsoft.JSInterop;

namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        // Scroll-related fields
        private bool checkScrollToBotoom = false;
        private bool isthrottled = false;
        private bool checkScrollToTop = false;

        private async Task ScrollToBottom()
        {
            try
            {
                await Task.Delay(1);
                await JS.InvokeVoidAsync("scrollToBottom", _messagesContainer);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error scrolling to bottom: {ex.Message}");
            }
        }

        private async Task HandleScroll()
        {
            if (isthrottled)
                return;

            isthrottled = true;
            await Task.Delay(150);

            if (!checkScrollToBotoom)
            {
                var isAtBottom = await JS.InvokeAsync<bool>("isScrollAtBottom", _messagesContainer);
                if (isAtBottom)
                {
                    Console.WriteLine("Scroll is at the bottom.");
                    checkScrollToBotoom = true;
                    MessageManager.UpdateLastSeen(_currentRoomId);
                }
                isthrottled = false;
                return;
            }

            if (!checkScrollToTop)
            {
                var isAtTop = await JS.InvokeAsync<bool>("isScrollAtTop", _messagesContainer);
                if (isAtTop)
                {
                    Console.WriteLine("Scroll is at the top.");
                    checkScrollToTop = true;
                    await TriggerLoadOlderHistoryRequest();
                    if (_currentBucketIndex != 0)
                        checkScrollToTop = false;
                }
            }

            isthrottled = false;
        }
        private async Task<string?> GetTopVisibleMessageId()
        {
            try
            {
                var messageId = await JS.InvokeAsync<string?>(
                    "getTopVisibleMessageId",
                    ".messages-container",
                    "message-"
                );
                return messageId;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"JS error: {ex.Message}");
                return null;
            }
        }
        private async Task ScrollToMessage(string id)
        {
            await JS.InvokeVoidAsync("scrollToMessage", id);
            _showContextMenu = false;
        }

        private async Task ScrollToContextMessage()
        {
            await ScrollToMessage(_contextMenuMessageId);
        }
    }
}
