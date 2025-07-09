using iChat.BackEnd.Models.MessageSearch;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IMessageSearchService
    {
        Task<PaginatedResult<ChatMessageDtoSafeSearchExtended>> SearchMessagesAsync(SearchOptions options);
        Task<int> GetSearchResultCountAsync(string queryText, long channelId, SearchOptions options = null);
        Task InvalidateSearchCacheAsync(long channelId);
    }
}
