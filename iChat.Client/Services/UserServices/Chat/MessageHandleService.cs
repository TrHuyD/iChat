using iChat.Client.Services.Auth;
using iChat.Client.Services.UI;
using iChat.DTOs.Users.Messages;
using System.Net.Http.Json;

namespace iChat.Client.Services.UserServices.Chat
{
    public class MessageHandleService
    {
        JwtAuthHandler _jwtAuthHandler;
        ToastService _toastService;
        public MessageHandleService(JwtAuthHandler jwtAuthHandler, ToastService toastService)
        {
            _jwtAuthHandler = jwtAuthHandler;
            _toastService = toastService;
        }
        public async Task DeleteMessageAsync(UserDeleteMessageRq deleteMessageRq)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat/DeleteMessage")
                {
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(deleteMessageRq), System.Text.Encoding.UTF8, "application/json")
                };
                var response = await _jwtAuthHandler.SendAuthAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _toastService.ShowError($"Failed to delete message: {errorContent}");
                    return;
                }

            }
            catch (Exception ex)
            {
                _toastService.ShowError($"An error occurred while deleting the message: {ex.Message}");
            }
        }
        public async Task EditMessageAsync(UserEditMessageRq editMessageRq)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat/EditMessage")
                {
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(editMessageRq), System.Text.Encoding.UTF8, "application/json")
                };
                var response = await _jwtAuthHandler.SendAuthAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _toastService.ShowError($"Failed to edit message: {errorContent}");
                    return;
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError($"An error occurred while editing the message: {ex.Message}");
            }
        }
    }
}
