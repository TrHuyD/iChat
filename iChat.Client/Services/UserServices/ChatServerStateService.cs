using iChat.DTOs.Users.Messages;

namespace iChat.Client.Services.UserServices
{
    public class ChatServerStateService
    {
        private List<ChatServerDto>? _chatServers;
        public List<ChatServerDto> Get()
        {
            if (_chatServers == null)
                throw new InvalidOperationException("Chat servers not loaded");
            return _chatServers;
        }
    }
}
