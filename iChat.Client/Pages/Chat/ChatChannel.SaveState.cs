namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        Dictionary<string, SavedState> _savedStates = new();
       private void SaveState()
        {
            if (string.IsNullOrEmpty(_currentRoomId) || string.IsNullOrEmpty(_currentServerId))
                return;
            if (!_savedStates.TryGetValue(_currentRoomId, out var savedState))
            {
                savedState= _savedStates[_currentRoomId] = new SavedState();
            }
            savedState._showSearchSidebar = _showSearchSidebar;
            SaveSearchState(savedState);
        }
        private void LoadState()
        {
            if (string.IsNullOrEmpty(_currentRoomId) || string.IsNullOrEmpty(_currentServerId))
                return;
            if (!_savedStates.TryGetValue(_currentRoomId, out var savedState))
            {
                savedState= _savedStates[_currentRoomId] = new();
            }
            LoadSearchState(savedState);
        }
        private partial class SavedState
        {
            
        }
    }
}
