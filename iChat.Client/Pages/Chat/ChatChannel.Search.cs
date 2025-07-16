using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;

namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {

        private bool _showSearchSidebar = false;
        private string _searchQuery = string.Empty;
        private IList<ChatMessageDtoSafeSearchExtended> _searchResults = new List<ChatMessageDtoSafeSearchExtended>();
        private CancellationTokenSource _searchCts = new();

        private string _searchSenderId = string.Empty;
        private string _searchChannelId = string.Empty;
        private DateTime? _searchBefore=null;
        private DateTime? _searchAfter=null;
        private string _sortBy = "timestamp";
        private bool _sortDescending = true;
        private int _currentSearchPage = 1;
        private int _totalSearchPages = 1;
        private partial class SavedStateServer
        {
            public bool _showSearchSidebar=false;
            public string _searchQuery = string.Empty;
            public string _searchSenderId = string.Empty;
            public string _searchChannelId = string.Empty;
            public DateTime? _searchBefore = null;
            public DateTime? _searchAfter = null;
            public string _sortBy = "timestamp";
            public bool _sortDescending = true;
            public IList<ChatMessageDtoSafeSearchExtended> _searchResults = new List<ChatMessageDtoSafeSearchExtended>();
            public int _currentSearchPage = 1;
            public int _totalSearchPages = 1;
        }
        private void SaveSearchState(SavedStateServer state)
        {
            state._showSearchSidebar = _showSearchSidebar;
            state._searchQuery = _searchQuery;
            state._searchSenderId = _searchSenderId;
            state._searchChannelId = _searchChannelId;
            state._searchBefore = _searchBefore;
            state._searchAfter = _searchAfter;
            state._sortBy = _sortBy;
            state._sortDescending = _sortDescending;
            state._searchResults = _searchResults;
            state._currentSearchPage= _currentSearchPage;
            state._totalSearchPages= _totalSearchPages;
        }
        private void LoadSearchState(SavedStateServer state)
        {
            _showSearchSidebar = state._showSearchSidebar;
            _searchQuery = state._searchQuery;
            _searchSenderId = state._searchSenderId;
            _searchChannelId = state._searchChannelId;
            _searchBefore = state._searchBefore;
            _searchAfter = state._searchAfter;
            _sortBy = state._sortBy;
            _sortDescending = state._sortDescending;
            _searchResults = state._searchResults;
            _totalSearchPages = state._totalSearchPages;
            _currentSearchPage = state._currentSearchPage;

        }
        private void ToggleSearchSidebar()
        {
            _showSearchSidebar = !_showSearchSidebar;
            _searchChannelId = _currentRoomId;
            _searchQuery = string.Empty;
            _searchResults.Clear();
        }
        private async Task TriggerSearchAsync(int page = 1)
        {
            _currentSearchPage = page;
            _searchCts.Cancel();
            _searchCts = new CancellationTokenSource();

            var query = _searchQuery.Trim();
            if (string.IsNullOrEmpty(query))
            {
                _searchResults.Clear();
                return;
            }

            try
            {
                var uri = $"/api/search/messages?" +
                          $"serverId={ServerId}" +
                          $"&channelId={_searchChannelId}" +
                          $"&queryText={Uri.EscapeDataString(query)}" +
                          $"&page={_currentSearchPage}&pageSize=20" +
                          $"&sortBy={_sortBy}" +
                          $"&sortDescending={_sortDescending}" +
                          (_searchBefore.HasValue ? $"&toDate={_searchBefore:O}" : "") +
                          (_searchAfter.HasValue ? $"&fromDate={_searchAfter:O}" : "");
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                using var response = await _https.SendAuthAsync(request);

                if (!response.IsSuccessStatusCode) return;

                var paged = await response.Content.ReadFromJsonAsync<PaginatedResult<ChatMessageDtoSafeSearchExtended>>();
                if (paged?.Items != null)
                {
                    _searchResults = paged.Items;
                    _totalSearchPages = paged.TotalPages;
                }
            }
            catch (TaskCanceledException) { }
        }




        private async Task HandleSearchKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await TriggerSearchAsync();
            }
        }
    }
}
