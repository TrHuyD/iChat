using iChat.Client.Data.Chat;
using iChat.Client.Services.Auth;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR.Client;

namespace iChat.Client.Services.UserServices.Chat
{
    public class ChatSignalRClientService : IAsyncDisposable
    {
        private readonly SignalRConnectionFactory _connectionFactory;
        private HubConnection? _hubConnection;
#if DEBUG
        private const string ChatHubPath = "https://localhost:6051/api/chathub";
#else
         private const string ChatHubPath = "/api/chathub";
#endif
        private readonly ChatMessageCacheService _MessageCacheService;
        private readonly ChatNavigationService _chatNavigationService;
        private readonly UserMetadataService _userMetadata;
        public ChatSignalRClientService(SignalRConnectionFactory connectionFactory,ChatMessageCacheService MessageCacheService, ChatNavigationService chatNavigationService, UserMetadataService userMetadata)
        {
            _MessageCacheService = MessageCacheService;
            _connectionFactory = connectionFactory;
            _chatNavigationService = chatNavigationService;
            _userMetadata = userMetadata;
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                return;

            _hubConnection = _connectionFactory.CreateHubConnection(ChatHubPath);

            Console.WriteLine("Registering ReceiveMessage on Signalr Client");
            _hubConnection.On<NewMessage>("ReceiveMessage", async message =>
            {
                try
                {
                    Console.WriteLine($"Recieved message for {message.message.Id}");
                    var newmessage = new ChatMessageDto(message.message);
                    _userMetadata.SyncMetadataVersion(newmessage.SenderId, long.Parse(message.UserMetadataVersion));
                    await _MessageCacheService.AddLatestMessage(newmessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling new message: {ex.Message}");
                }
            });
            _hubConnection.On<ChatChannelDto>("ChannelCreate", HandleChannelCreate);
            _hubConnection.On<DeleteMessageRt>("MessageDelete",async rt =>
            {
                try
                {
                   await HandleDeleteMessage(rt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling delete message: {ex.Message}");
                }
            });
            _hubConnection.On<UserMetadata>("UpdateProfile", async rt =>
            {
                try
                {
                    HandleMetadataChange(rt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling delete message: {ex.Message}");
                }
            });
            _hubConnection.On<string>("LeaveServer",LeaveRoomAsync);
            _hubConnection.On<ChatServerMetadata>("JoinNewServer", OnJoiningNewServer);
            _hubConnection.On<EditMessageRt>("MessageEdit",  HandleEditMessage);
            _hubConnection.On<string, string>("UserTyping", async (channelId, userId) =>
            {
                try
                {
                    OnUserType(channelId,userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling user type: {ex.Message}");
                }
            });
            _hubConnection.Closed += async (error) =>
            {
                try
                {
                    Console.WriteLine("Disconnected from ChatHub");
                    await Task.Delay(50);
                    await ConnectAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error editing channel: {ex.Message}");
                }
            }
                ;


            await _hubConnection.StartAsync();
            Console.WriteLine("Connected to ChatHub");
        }
        public async Task JoinRoomAsync(string roomId)
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                await _hubConnection.InvokeAsync("JoinRoom", roomId);
            }
        }
        public async Task LeaveRoomAsync(string roomId)
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                await _hubConnection.InvokeAsync("LeaveRoom", roomId);
            }
            _chatNavigationService.RemoveServer(roomId);

        }
        public async Task SendMessageAsync(string roomId, ChatMessageDtoSafe message)
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                await _hubConnection.InvokeAsync("SendMessage", roomId, message);
            }
        }
        public async Task Typing()
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                await _hubConnection.InvokeAsync("Typing");
            }
        }
        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
        }
        private async Task HandleDeleteMessage(DeleteMessageRt rt)
        {

         await   _MessageCacheService.HandleDeleteMessage(rt); 

        }
        private async Task HandleEditMessage(EditMessageRt editMessage)
        {
            await _MessageCacheService.HandleEditMessage(editMessage);
        }
        private void HandleChannelCreate(ChatChannelDto channel)
        {
            _chatNavigationService.AddChannel(channel);
        }
        private void HandleMetadataChange(UserMetadata userMetadata)
        {
            _userMetadata.SetUserProfile(userMetadata);
        }
        private void OnJoiningNewServer(ChatServerMetadata serverID)
        {
            _chatNavigationService.AddServer(serverID);
        }

        public event Action<(string channelId, string userId)>? TypingReceived;
        private void OnUserType(string channelId, string userId)
        {
            TypingReceived?.Invoke((channelId, userId));
        }

    }

}
