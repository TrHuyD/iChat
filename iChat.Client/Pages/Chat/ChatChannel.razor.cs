using iChat.Client.Data;
using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices;
using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
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
        private string?_currentServerId;
        private ChatServerDtoUser? _currentServer=new ChatServerDtoUser();
        private ChatChannelDtoLite? _currentChannel= new ChatChannelDtoLite();
        private string _newMessage = string.Empty;
        private ElementReference _messagesContainer;
        private string _connectionStatus = "Disconnected";
        private string _connectionStatusClass = "disconnected";
        private string _currentUserId = "";
        private bool _shouldScrollToBottom = false;
        private readonly Channel<ChatMessageDtoSafe> _sendQueue = Channel.CreateUnbounded<ChatMessageDtoSafe>();

        private Task? _sendQueueTask;
        private SortedList<long, RenderedMessage> _messages = new();
        private List<MessageGroup> _groupedMessages = new();
        bool Init_failed = false;
        protected override async Task OnInitializedAsync()
        {
            if (!_userInfo.ConfirmServerChannelId(ServerId, RoomId))
            {
                Console.WriteLine($"ServerId {ServerId} does not match the expected server for RoomId {RoomId}.");
                Init_failed = true;
                _navigation.NavigateTo("/");
                return;
            }
            Console.WriteLine($"Initializing ChatChannel for RoomId: {RoomId}");
           _userMetadataService._onMetadataUpdated += HandleUserMetadataUpdate;
            _sendQueueTask = ProcessSendQueueAsync();
            _currentUserId = _userInfo.GetUserId().ToString();
            try
            {
                Console.WriteLine("Registered message handler for ChatService.");
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
        private void HandleUserMetadataUpdate(List<UserMetadataReact> updatedUsers)
        {
            InvokeAsync(StateHasChanged);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Init_failed)
                return;
            if (_currentRoomId != RoomId)
            {
                SaveState();
                if (!string.IsNullOrEmpty(_currentRoomId))
                    await ChatService.LeaveRoomAsync(_currentRoomId);
                if(!string.IsNullOrEmpty(_currentServerId)|| _currentServerId != ServerId)
                {
                    _currentServerId = ServerId;
                    _currentServer = _ServerCacheManager.GetServer(_currentServerId);
                  await  _ServerCacheManager.OnServerChange(ServerId);
                }
                await _ServerCacheManager.OnChannelChange(RoomId);
                _currentChannel =_currentServer.Channels.FirstOrDefault(c => c.Id == RoomId);
                _currentRoomId = RoomId;
                LoadState();
                checkScrollToBotoom = false;
                var (latest, loc) = await MessageManager.GetLatestMessage(RoomId);
                _messages.Clear();
                MessageManager.RegisterOnMessageReceived(HandleNewMessage);
                Console.WriteLine("Registered message handler for ChatService.");
                foreach (var bucket in latest)
                    await AddMessages(bucket);
                _currentBucketIndex = latest[0].BucketId;
                await ChatService.JoinRoomAsync(ServerId);
                StateHasChanged();
                checkScrollToBotoom = checkScrollToTop = _isOldHistoryRequestButtonDisabled = checkScrollToTop = _currentBucketIndex == 0;
                await Task.Delay(125);
                await ScrollToMessage(loc);
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

        private async Task HandleNewMessage(ChatMessageDtoSafe message)
        {
            if (message.ChannelId != RoomId) return;
            try
            {
                var messageId = long.Parse(message.Id);
                var rendered = MessageRenderer.RenderMessage(message, _currentUserId);
                _messages.TryAdd(messageId, rendered);

                await TryAddNewMessageToGroupAsync(rendered);
                _shouldScrollToBottom = true;

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling new message: {ex.Message}");
            }
        }
        public async ValueTask DisposeAsync()
        {
            await DisposeCore();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            _userMetadataService._onMetadataUpdated -= HandleUserMetadataUpdate;
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
    }
}