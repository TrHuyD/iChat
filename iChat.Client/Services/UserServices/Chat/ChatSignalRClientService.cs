using iChat.Client.Data.Chat;
using iChat.Client.Services.Auth;
using iChat.Client.Services.UI;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Enum;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Channels;

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
        private readonly ToastService _toastService;
        private readonly UserMetadataService _userMetadata;
        public event Action<MemberList>? OnlineListRecieved;
        public event Action<(stringlong id, bool online)>? OnlineStatechanged;
        public event Action<long> UserTypingReceived;
        public void ResigerOnUserTypingRecieved(Action<long> callback) => UserTypingReceived += callback; 
        public void RegisterOnMemberListRecieve(Action<MemberList> callback) => OnlineListRecieved += callback;
        public void RegisterOnOnlineUpdate(Action<(stringlong id,bool online)> callback)=> OnlineStatechanged += callback;
        public ChatSignalRClientService(SignalRConnectionFactory connectionFactory,ToastService toastService,ChatMessageCacheService MessageCacheService, ChatNavigationService chatNavigationService, UserMetadataService userMetadata)
        {
            _toastService = toastService;
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
            _hubConnection.On<NewMessage>(SignalrClientPath.RecieveMessage, async message =>
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
            _hubConnection.On<ChatChannelDto>(SignalrClientPath.ChannelCreate, HandleChannelCreate);
            _hubConnection.On<DeleteMessageRt>(SignalrClientPath.MessageDelete,async rt =>
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
            _hubConnection.On<UserMetadata>(SignalrClientPath.UpdateProfile, async rt =>
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
            _hubConnection.On<string>(SignalrClientPath.LeaverServer, ForceLeaveRoom);
            _hubConnection.On<ChatServerMetadata>(SignalrClientPath.JoinNewServer, OnJoiningNewServer);
            _hubConnection.On<EditMessageRt>(SignalrClientPath.MessageEdit,  HandleEditMessage);
            _hubConnection.On<long>(SignalrClientPath.UserTyping,  (userId) =>
            {
                try
                {
                    OnUserType(userId);
                    Console.WriteLine($"{userId} is typing");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling user type: {ex.Message}");
                }
            });
            _hubConnection.On<MemberList>(SignalrClientPath.UserList, (userlist)=> 
              {
                  try
                  {
                      Console.WriteLine($"Handling user list");

                      OnlineListRecieved?.Invoke(userlist);
                  }
                  catch(Exception ex)
                  {
                      Console.WriteLine($"Error handling user list {ex.Message}");
                  }
            });
            _hubConnection.On<stringlong>(SignalrClientPath.NewUserOnline, (id) =>
            {
            try
                {
                    Console.WriteLine($"User {id} online");
                    OnlineStatechanged?.Invoke((id, true));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling user online {ex.Message}");

                }
            }
            );
            _hubConnection.On<stringlong>(SignalrClientPath.NewUserOffline, (id) =>
            {
                try
                {
                    Console.WriteLine($"User {id} offline");
                    OnlineStatechanged?.Invoke((id, false));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling user offline {ex.Message}");

                }
            }
);
            _hubConnection.On<ChatServerChangeUpdate>(SignalrClientPath.ServerProfileChange, (update)=> OnChatServerProfileUpdate(update));
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
            } ;


            await _hubConnection.StartAsync();
            Console.WriteLine("Connected to ChatHub");
        }
        public async Task NotifyJoinServer(ChatServerConnectionState state)
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
               var result =await _hubConnection.InvokeAsync<bool>("JoinRoom", state);
                if(!result)
                {
                    _toastService.ShowError("Error when joining server");
                    _chatNavigationService.NavigateToHome();
                }
                    
            }
        }
        public async Task NotifyJoinChannel(string ChannelId)
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                var result = await _hubConnection.InvokeAsync<bool>("JoinChannel", ChannelId);
                if (!result)
                {
                    _toastService.ShowError("Error when joining channel ");
                    _chatNavigationService.NavigateToHome();
                }
            }

        }
        public async Task ForceLeaveRoom(string ServerId)
        {
            _chatNavigationService.RemoveServer(ServerId);

        }
        public async Task NotifyLeaveRoom()
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
                await _hubConnection.SendAsync("LeaveRoom");

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
                await _hubConnection.SendAsync("Typing");
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

        private void OnUserType(long userId)
        {
            UserTypingReceived?.Invoke(userId);
        }
        private void OnChatServerProfileUpdate(ChatServerChangeUpdate update)
        {
            _chatNavigationService.UpdateServer(update);

        }
    }

}
