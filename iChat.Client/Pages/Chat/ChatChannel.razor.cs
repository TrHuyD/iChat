using iChat.Client.Data;
using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices;
using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Threading.Channels;
using System.Timers;

namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel : ComponentBase, IDisposable, IAsyncDisposable
    {
        [Parameter] public string ChannelId { get; set; } = string.Empty;
        [Parameter] public string ServerId { get; set; } = string.Empty;
        public long RoomIdL;
        public long ServerIdL;
        private string? _currentRoomId;
        private string?_currentServerId;
        private ChatServerDtoUser? _currentServer=new ChatServerDtoUser();
        private ChatChannelDtoLite? _currentChannel= new ChatChannelDtoLite();
        private string _newMessage = string.Empty;
        private int _myCount = 0;
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
            if (!_userInfo.ConfirmServerChannelId(ServerId, ChannelId))
            {
                Console.WriteLine($"ServerId {ServerId} does not match the expected server for RoomId {ChannelId}.");
                Init_failed = true;
                _navigation.NavigateTo("/");
                return;
            }
            Console.WriteLine($"Initializing ChatChannel for RoomId: {ChannelId}");
           _userMetadataService.OnMetadataUpdated += HandleUserMetadataUpdate;
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
        private void HandleUserMetadataUpdate()
        {
            InvokeAsync(StateHasChanged);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Init_failed)
                return;
            if (_currentRoomId != ChannelId)
            {
                RoomIdL= long.Parse(ChannelId);
                ServerIdL = long.Parse(ServerId);
                SaveState();
                if (!string.IsNullOrEmpty(_currentRoomId))
                    await ChatService.LeaveRoomAsync(_currentRoomId);
                if(!string.IsNullOrEmpty(_currentServerId)|| _currentServerId != ServerId)
                {
                    _currentServerId = ServerId;
                    _currentServer = _ServerCacheManager.GetServer(_currentServerId);
                  await  _ServerCacheManager.OnServerChange(ServerId);
                }
                await _ServerCacheManager.OnChannelChange(ChannelId);
                _currentChannel =_currentServer.Channels.FirstOrDefault(c => c.Id == ChannelId);
                _currentRoomId = ChannelId;
                LoadState();
                checkScrollToBotoom = false;
                var (latest, loc) = await MessageManager.GetLatestMessage(ChannelId);
                _messages.Clear();
                MessageManager.RegisterOnMessageReceived(HandleNewMessage);
                MessageManager.RegisterOnMessageEdited(HandleEditMessage);
                MessageManager.RegisterOnMessageDeleted(HandleDeleteMessage);
                Console.WriteLine("Registered message handler for ChatService.");
                _groupedMessages.Clear();
                foreach (var bucket in latest)
                    await AddMessagesForward(bucket);
                _currentBucketIndex = latest[0].BucketId;
                await ChatService.JoinRoomAsync(ServerId);
                StateHasChanged();
                checkScrollToBotoom = checkScrollToTop = _isOldHistoryRequestButtonDisabled = checkScrollToTop = _currentBucketIndex == 0;


                ///Typing
                _typingTimer = new System.Timers.Timer(1000);
                _typingTimer.Elapsed += (_, _) =>
                {
                    var now = DateTime.UtcNow;
                    var expired = _typingUsers
                        .Where(x => (now - x.Value).TotalSeconds > 3)
                        .Select(x => x.Key)
                        .ToList();

                    foreach (var uid in expired)
                        _typingUsers.Remove(uid);

                    InvokeAsync(StateHasChanged);
                };
                _typingTimer.Start();
                //ChatService.TypingReceived += HandleTyping;


                await Task.Delay(125);
                await ScrollToMessage(loc.ToString());
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
        private async Task HandleDeleteMessage( DeleteMessageRt rq)
        {
            if(rq.ChannelId != ChannelId) return;
            try
            {
                var messageId = long.Parse(rq.MessageId);

                if (_messages.TryGetValue(messageId, out var oldMessage))
                {
                    _messages[messageId] = oldMessage.WithDelete();
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling delete message: {ex.Message}");
            }
        }
        private async Task HandleEditMessage(EditMessageRt rq)
        {
            if (rq.ChannelId != ChannelId) return;
            try
            {
                var messageId = long.Parse(rq.MessageId);

                if (_messages.TryGetValue(messageId, out var oldMessage))
                {
                    _messages[messageId] = oldMessage.WithEdit(rq.NewContent);
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling edit message: {ex.Message}");
            }
        }
        private async Task HandleNewMessage(ChatMessageDto message)
        {
            if (message.ChannelId !=  RoomIdL) return;
            try
            {
                var messageId = message.Id;
                var rendered = MessageRenderer.RenderMessage(message);
                _messages.TryAdd(messageId, rendered);

                await TryAddNewMessageToGroupAsync(rendered);
                if(rendered.Message.SenderId.ToString()==_currentUserId)
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
            //ChatService.TypingReceived -= HandleTyping;
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            _userMetadataService.OnMetadataUpdated -= HandleUserMetadataUpdate;
            DisposeCore().AsTask().Wait();
            GC.SuppressFinalize(this);
        }
        private Dictionary<long, DateTime> _typingUsers = new();
        private System.Timers.Timer? _typingTimer;

        private bool _hasSentTyping = false;
        private System.Timers.Timer? _typingDebounceTimer;
        private readonly object _typingLock = new();

        //private async Task HandleInput(ChangeEventArgs e)
        //{
        //    _newMessage = e.Value?.ToString() ?? "";

        //    if (!_hasSentTyping)
        //    {
        //        _hasSentTyping = true;
        //        await ChatService.Typing();
        //    }

        //    ResetTypingDebounceTimer();
        //}
        //private void ResetTypingDebounceTimer()
        //{
        //    lock (_typingLock)
        //    {
        //        if (_typingDebounceTimer != null)
        //        {
        //            _typingDebounceTimer.Stop();
        //            _typingDebounceTimer.Elapsed -= OnTypingDebounceElapsed;
        //        }
        //        else
        //        {
        //            _typingDebounceTimer = new System.Timers.Timer(3000);
        //            _typingDebounceTimer.AutoReset = false;
        //        }

        //        _typingDebounceTimer.Elapsed += OnTypingDebounceElapsed;
        //        _typingDebounceTimer.Start();
        //    }
        //}
        //private void OnTypingDebounceElapsed(object? sender, ElapsedEventArgs e)
        //{
        //    _hasSentTyping = false;

        //    lock (_typingLock)
        //    {
        //        if (_typingDebounceTimer != null)
        //        {
        //            _typingDebounceTimer.Elapsed -= OnTypingDebounceElapsed;
        //            _typingDebounceTimer.Stop();
        //        }
        //    }
        //}

        //private void HandleTyping((string channelId, string userId) package)
        //{
        //    if (package.channelId != _currentChannel.Id) return;

        //    if (long.TryParse(package.userId, out var uid))
        //    {
        //        _typingUsers[uid] = DateTime.UtcNow;
        //        InvokeAsync(StateHasChanged);
        //    }
        //}


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