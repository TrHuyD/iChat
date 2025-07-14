﻿namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        Dictionary<string, SavedStateServer> _savedStateserver = new();
        Dictionary<string, SavedStateChannel> _savedStatesChannel = new();
        private void SaveState()
        {
            if (string.IsNullOrEmpty(_currentRoomId) || string.IsNullOrEmpty(_currentServerId))
                return;
            if (!_savedStateserver.TryGetValue(_currentServerId, out var savedStateServer))
            {
                savedStateServer = _savedStateserver[_currentServerId] = new ();
            }
            if (!_savedStatesChannel.TryGetValue(_currentRoomId, out var savedStateChannel))
            {
                savedStateChannel = _savedStatesChannel[_currentRoomId] = new();
            }
            SaveSearchState(savedStateServer);
            
        }
        private void LoadState()
        {
            if (string.IsNullOrEmpty(_currentRoomId) || string.IsNullOrEmpty(_currentServerId))
                return;
            if (!_savedStateserver.TryGetValue(_currentServerId, out var savedStateServer))
            {
                savedStateServer = _savedStateserver[_currentServerId] = new();
            }
            if (!_savedStatesChannel.TryGetValue(_currentRoomId, out var savedStateChannel))
            {
                savedStateChannel = _savedStatesChannel[_currentRoomId] = new();
            }
            if (_currentServerId != ServerId)
            {
                LoadSearchState(savedStateServer);
            }
        }
        private partial class SavedStateChannel
        {
            
        }
    }
}
