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
        private DateTime? _searchBefore;
        private DateTime? _searchAfter;
        private string _sortBy = "timestamp";
        private bool _sortDescending = true;

        private void ToggleSearchSidebar()
        {
            _showSearchSidebar = !_showSearchSidebar;
            _searchChannelId = _currentRoomId;
            _searchQuery = string.Empty;
            _searchResults.Clear();
        }
        private async Task TriggerSearchAsync()
        {
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
                          $"&page=1&pageSize=20" +
                          $"&senderId={Uri.EscapeDataString(_searchSenderId ?? "")}" +
                          $"&sortBy={_sortBy}" +
                          $"&sortDescending={_sortDescending}" +
                          (_searchBefore.HasValue ? $"&toDate={_searchBefore:O}" : "") +
                          (_searchAfter.HasValue ? $"&fromDate={_searchAfter:O}" : "");
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                using var response = await _https.SendAuthAsync(request);

                if (!response.IsSuccessStatusCode)
                    return;

                var paged = await response.Content.ReadFromJsonAsync<PaginatedResult<ChatMessageDtoSafeSearchExtended>>();
                if (paged?.Items != null)
                    _searchResults = paged.Items;
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Search error: {ex.Message}");
            }
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
