namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        private int _typingCount = 0; 
        void OnUserStartedTyping() => _typingCount++;
        void OnUserStoppedTyping() => _typingCount = Math.Max(0, _typingCount - 1);

    }
}
