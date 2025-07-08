using iChat.Client.Data;
using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Threading.Channels;

namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel : ComponentBase, IDisposable, IAsyncDisposable
    {
        [Parameter] public string RoomId { get; set; } = string.Empty;
        [Parameter] public string ServerId { get; set; } = string.Empty;
        private string? _currentRoomId;
        private string _newMessage = string.Empty;
        private ElementReference _messagesContainer;
        private string _connectionStatus = "Disconnected";
        private string _connectionStatusClass = "disconnected";
        private string _currentUserId = "";
        private bool _shouldScrollToBottom = false;
        private readonly Channel<ChatMessageDtoSafe> _sendQueue = Channel.CreateUnbounded<ChatMessageDtoSafe>();
        private Task? _sendQueueTask;
        private SortedList<long, RenderedMessage> _messages = new();
        // Context menu state
        private bool _showContextMenu = false;
        private string _contextMenuStyle = "";
        private string _contextMenuMessageId;

        //loading button state
        private bool _isLoading = false;
        private bool _isOldHistoryRequestButtonDisabled = false;

        //bucket stuff
        private int _currentBucketIndex = 0;

        private async Task TriggerLoadOlderHistoryRequest()
        {
            if (_isLoading || _isOldHistoryRequestButtonDisabled)
                return;

            _isLoading = true;
            try
            {
                var result = await MessageManager.GetPreviousBucket(RoomId, _currentBucketIndex);
                try
                {
                    await AddMessages(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                _currentBucketIndex = result.BucketId;
                Console.WriteLine($"Loaded previous bucket with ID: {_currentBucketIndex}");
                if (_currentBucketIndex == 0)
                {
                    DisableSpecialButtonPermanently();
                }
                Console.WriteLine("Special request completed.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Special request failed: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private void DisableSpecialButtonPermanently()
        {
            _isOldHistoryRequestButtonDisabled = true;
        }



        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine($"Initializing ChatChannel for RoomId: {RoomId}");

            if (!_userInfo.ConfirmServerChannelId(ServerId, RoomId))
            {
                Console.Error.WriteLine($"ServerId {ServerId} does not match the expected server for RoomId {RoomId}.");
                _navigation.NavigateTo("/");
            }
            _sendQueueTask = ProcessSendQueueAsync();
            _currentUserId = _userInfo.GetUserId().ToString();


            try
            {
                Console.WriteLine("Registed message handler for ChatService.");
                await ChatService.ConnectAsync();
                _connectionStatus = "Connected";
                _connectionStatusClass = "connected";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error connecting to chat: {ex.Message}");
                _connectionStatus = "Connection Failed";
                _connectionStatusClass = "disconnected";
            }

            StateHasChanged();

        }

        private async Task AddMessages(BucketDto bucket)
        {
            var previousScroll = await JS.InvokeAsync<ScrollSnapshot>("captureScrollAnchor", _messagesContainer);
            foreach (var message in bucket.ChatMessageDtos)
            {
                _messages.TryAdd(long.Parse(message.Id), MessageRenderer.RenderMessage(message, _currentUserId));
            }
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("restoreScrollAfterPrepend", _messagesContainer, previousScroll);
        }
        protected override async Task OnParametersSetAsync()
        {
            if (_currentRoomId != RoomId)
            {
                if (!string.IsNullOrEmpty(_currentRoomId))
                    await ChatService.LeaveRoomAsync(_currentRoomId);
                checkScrollToBotoom = false;
                _currentRoomId = RoomId;
                var (latest, loc) = await MessageManager.GetLatestMessage(RoomId);



                _messages.Clear();
                MessageManager.RegisterOnMessageReceived(HandleNewMessage);
                Console.WriteLine("Registed message handler for ChatService.");
                foreach (var bucket in latest)
                    AddMessages(bucket);
                _currentBucketIndex = latest[0].BucketId;

                await ChatService.JoinRoomAsync(ServerId);
                StateHasChanged();
                await Task.Delay(125);
                await ScrollToMessage(loc);
                checkScrollToTop = _currentBucketIndex == 0;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_shouldScrollToBottom)
            {
                await ScrollToBottom();
                _shouldScrollToBottom = false;
            }
        }

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

        private async Task HandleNewMessage(ChatMessageDtoSafe message)
        {
            try
            {
                if (message.ChannelId == RoomId)
                {
                    var messageId = long.Parse(message.Id);
                    checkScrollToBotoom = false;
                    _messages.TryAdd(long.Parse(message.Id), MessageRenderer.RenderMessage(message, _currentUserId));
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling new message: {ex.Message}");
            }
        }

        private async Task ProcessSendQueueAsync()
        {
            await foreach (var message in _sendQueue.Reader.ReadAllAsync())
            {
                try
                {
                    await ChatService.SendMessageAsync(ServerId, message);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to send message: {ex.Message}");
                }
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(_newMessage)) return;

            var messageContent = _newMessage.Trim();
            _newMessage = string.Empty;
            StateHasChanged();

            var message = new ChatMessageDtoSafe
            {
                Content = messageContent,
                MessageType = 1,
                ChannelId = RoomId,
                SenderId = _currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _sendQueue.Writer.WriteAsync(message);
            await Task.Delay(125);
            _shouldScrollToBottom = true;
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await SendMessage();
            }
        }
        bool checkScrollToBotoom = false;
        bool isthrottled = false;
        bool checkScrollToTop = false;
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
            return;

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

        public async ValueTask DisposeAsync()
        {
            await DisposeCore();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            DisposeCore().AsTask().Wait();
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeCore()
        {
            try
            {
                await ChatService.LeaveRoomAsync(ServerId);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error leaving room on dispose: {ex.Message}");
            }

            _sendQueue.Writer.Complete();
            if (_sendQueueTask != null)
            {
                await _sendQueueTask;
            }
        }

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
