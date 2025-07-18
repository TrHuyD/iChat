namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        Dictionary<string, SavedStateServer> _savedStateserver = new();
        Dictionary<string, SavedStateChannel> _savedStatesChannel = new();
        private void SaveState()
        {
            if (string.IsNullOrEmpty(_currentChannelId) || string.IsNullOrEmpty(_currentServerId))
                return;
            if (!_savedStateserver.TryGetValue(_currentServerId, out var savedStateServer))
            {
                savedStateServer = _savedStateserver[_currentServerId] = new ();
            }
            if (!_savedStatesChannel.TryGetValue(_currentChannelId, out var savedStateChannel))
            {
                savedStateChannel = _savedStatesChannel[_currentChannelId] = new();
            }
            SaveSearchState(savedStateServer);
            
        }
        private void LoadState(bool isNewServer)
        {
            if (string.IsNullOrEmpty(_currentChannelId) || string.IsNullOrEmpty(_currentServerId))
                return;

            if (!_savedStateserver.TryGetValue(_currentServerId, out var savedStateServer))
                _savedStateserver[_currentServerId] = savedStateServer = new();

            if (!_savedStatesChannel.TryGetValue(_currentChannelId, out var savedStateChannel))
                _savedStatesChannel[_currentChannelId] = savedStateChannel = new();

            if (isNewServer)
            {
                LoadSearchState(savedStateServer);
            }
        }

        private partial class SavedStateChannel
        {
            
        }
    }
}
