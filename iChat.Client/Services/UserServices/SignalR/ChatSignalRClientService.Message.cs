using iChat.Client.Data.Chat;
using iChat.Client.Services.Auth;
using iChat.Client.Services.UI;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Enum;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Channels;

namespace iChat.Client.Services.UserServices.SignalR
{
    public partial class ChatSignalRClientService
    {
        private void ConnectMessageAsync()
        {
            _hubConnection.On<EditMessageRt>(SignalrClientPath.MessageEdit, HandleEditMessage);
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
            _hubConnection.On<DeleteMessageRt>(SignalrClientPath.MessageDelete, async rt =>
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
        }
        public async Task SendMessageAsync(string roomId, ChatMessageDtoSafe message)
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                await _hubConnection.InvokeAsync("SendMessage", roomId, message);
            }
        }
        private async Task HandleDeleteMessage(DeleteMessageRt rt)
        {

            await _MessageCacheService.HandleDeleteMessage(rt);

        }
        private async Task HandleEditMessage(EditMessageRt editMessage)
        {
            await _MessageCacheService.HandleEditMessage(editMessage);
        }
    }

}
