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
        private void ToggleSearchSidebar()
        {
            _showSearchSidebar = !_showSearchSidebar;
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
                var uri = $"/api/search/messages?serverId={ServerId}&queryText={Uri.EscapeDataString(query)}&page=1&pageSize=10";
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
