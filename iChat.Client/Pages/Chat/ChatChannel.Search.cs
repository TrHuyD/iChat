namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        // Search fields
        private bool _showSearchSidebar = false;
        private string _searchQuery = string.Empty;
        private List<string> _searchResults = new();
        private void ToggleSearchSidebar()
        {
            _showSearchSidebar = !_showSearchSidebar;
            _searchQuery = string.Empty;
            _searchResults.Clear();
        }
    }
}
