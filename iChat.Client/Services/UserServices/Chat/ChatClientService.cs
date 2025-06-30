using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR.Client;

namespace iChat.Client.Services.UserServices.Chat
{
    public class ChatClientService : IAsyncDisposable
    {
        private readonly SignalRConnectionFactory _connectionFactory;
        private HubConnection? _hubConnection;
        public event Func<ChatMessageDtoSafe, Task>? OnMessageReceived;
        private const string ChatHubPath = "https://localhost:6051/api/chathub";
        private readonly ChatMessageService _MessageCacheService;
        public ChatClientService(SignalRConnectionFactory connectionFactory,ChatMessageService MessageCacheService)
        {
            _MessageCacheService = MessageCacheService;
            _connectionFactory = connectionFactory;
        }
        public async Task ConnectAsync()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                return;

            _hubConnection = _connectionFactory.CreateHubConnection(ChatHubPath);

            _hubConnection.On<ChatMessageDtoSafe>("ReceiveMessage", async message =>
            {
                if (OnMessageReceived != null)
                {
                    _MessageCacheService.AddLatestMessage(message.ChannelId, message);
                    await OnMessageReceived.Invoke(message);
                }
            });

            _hubConnection.Closed += async (error) =>
            {
                Console.WriteLine("Disconnected from ChatHub");
                await Task.Delay(50); 
                await ConnectAsync();   
            };

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
        }
        public async Task SendMessageAsync(string roomId, ChatMessageDtoSafe message)
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                await _hubConnection.InvokeAsync("SendMessage", roomId, message);
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
    }
}
