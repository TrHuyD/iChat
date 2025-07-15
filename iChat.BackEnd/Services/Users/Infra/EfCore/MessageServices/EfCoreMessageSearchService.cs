using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Models.MessageSearch;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NpgsqlTypes;
using System;
using System.Text;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreMessageSearchService:IMessageSearchService
    {
        private readonly iChatDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EfCoreMessageSearchService> _logger;
        private const int MaxPageSize = 100;
        private const int DefaultCacheMinutes = 5;
        public EfCoreMessageSearchService(iChatDbContext db, IMemoryCache cache, ILogger<EfCoreMessageSearchService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }
        public async Task<PaginatedResult<ChatMessageDtoSafeSearchExtended>> SearchMessagesAsync(SearchOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.QueryText))
                throw new ArgumentException("Query text cannot be empty", nameof(options.QueryText));
            if (options.PageSize > MaxPageSize)
                options.PageSize = MaxPageSize;
            if (options.Page < 1)
                options.Page = 1;
            var cacheKey = GenerateCacheKey(options);
            if (_cache.TryGetValue(cacheKey, out PaginatedResult<ChatMessageDtoSafeSearchExtended> cachedResult))
            {
                _logger.LogDebug($"Cache hit for search: {cacheKey}");
                return cachedResult;
            }
            try
            {
                var result = await ExecuteSearchAsync(options);
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(DefaultCacheMinutes),
                    Priority = CacheItemPriority.Normal,
                    Size = 1
                };
                _cache.Set(cacheKey, result, cacheEntryOptions);
                _logger.LogDebug($"Cached search result: {cacheKey}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing search for channel {ChannelId} with query '{Query}'",
                    options.ChannelId, options.QueryText);
                throw;
            }
        }
        private async Task<PaginatedResult<ChatMessageDtoSafeSearchExtended>> ExecuteSearchAsync(SearchOptions options)
        {
            string tsQueryText;
            try
            {
                var sanitizedQuery = SanitizeSearchQuery(options.QueryText);
                tsQueryText = $"{sanitizedQuery}:*";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sanitize query '{Query}', using exact match", options.QueryText);
                tsQueryText = options.QueryText;
            }
            if (!options.ChannelId.HasValue)
                throw new NotImplementedException("Havent implemented search for all channels yet, please specify a channelId.");
            var baseQuery = _db.Messages
                .Where(m => m.ChannelId == options.ChannelId)
                .Where(m => m.SearchVector.Matches(EF.Functions.ToTsQuery("english", tsQueryText)));
            if (!string.IsNullOrEmpty(options.SenderId) &&
                long.TryParse(options.SenderId, out long senderId))
            {
                baseQuery = baseQuery.Where(m => m.SenderId == senderId);
            }
            if (options.FromDate.HasValue)
                baseQuery = baseQuery.Where(m => m.Timestamp >= options.FromDate.Value);
            if (options.ToDate.HasValue)
                baseQuery = baseQuery.Where(m => m.Timestamp <= options.ToDate.Value);
            baseQuery = ApplySorting(baseQuery, options, tsQueryText);
            if (!string.IsNullOrEmpty(options.CursorToken) && TryDecodeCursor(options.CursorToken, out var cursorTimestamp))
            {
                baseQuery = options.SortDescending
                    ? baseQuery.Where(m => m.Timestamp < cursorTimestamp)
                    : baseQuery.Where(m => m.Timestamp > cursorTimestamp);
            }
            else if (options.Page > 1)
            {
                baseQuery = baseQuery.Skip((options.Page - 1) * options.PageSize);
            }
            var items = await baseQuery
                .Take(options.PageSize + 1)
                .ToListAsync();
            var hasNextPage = items.Count > options.PageSize;
            if (hasNextPage)
                items.RemoveAt(items.Count - 1);
            string nextPageToken = hasNextPage ? EncodeCursor(items.Last().Timestamp) : null;
            string previousPageToken = (options.Page > 1 && items.Count > 0)
                ? EncodeCursor(items.First().Timestamp)
                : null;
            return new PaginatedResult<ChatMessageDtoSafeSearchExtended>
            {
                Items = items.Select(i => new ChatMessageDtoSafeSearchExtended
                {
                    Id = i.Id.ToString(),
                    ChannelId = i.ChannelId.ToString(),
                    SenderId = i.SenderId.ToString(),
                    Content = i.TextContent,
                    CreatedAt = i.Timestamp,
                    ContentMedia = i.MediaFile.ToDto()
                }).ToList(),
                CurrentPage = options.Page,
                PageSize = options.PageSize,
                HasNextPage = hasNextPage,
                HasPreviousPage = options.Page > 1,
                NextPageToken = nextPageToken,
                PreviousPageToken = previousPageToken
            };
        }
        private IQueryable<Message> ApplySorting(IQueryable<Message> query, SearchOptions options, string tsQueryText)
        {
            return options.SortBy?.ToLower() switch
            {
                "timestamp" => options.SortDescending
                    ? query.OrderByDescending(m => m.Timestamp)
                    : query.OrderBy(m => m.Timestamp),

                "relevance" => query.OrderByDescending(m =>
                    m.SearchVector.Rank(EF.Functions.ToTsQuery("english", tsQueryText))),

                _ => query.OrderByDescending(m => m.Timestamp)
            };
        }
        public async Task<int> GetSearchResultCountAsync(string queryText, long channelId, SearchOptions options = null)
        {
            var countCacheKey = $"search_count:{channelId}:{queryText.GetHashCode()}";
            if (_cache.TryGetValue(countCacheKey, out int cachedCount))
                return cachedCount;
            try
            {
                var tsQuery = EF.Functions.ToTsQuery("english", SanitizeSearchQuery(queryText) + ":*");
                var query = _db.Messages
                    .Where(m => m.ChannelId == channelId && m.SearchVector.Matches(tsQuery));
                if (options != null)
                {
                    if (!long.TryParse(options.SenderId, out long SenderId))
                        throw new ArgumentException("Invalid User ID format", nameof(options.SenderId));
                    if (!string.IsNullOrEmpty(options.SenderId))
                        query = query.Where(m => m.SenderId == SenderId);
                    if (options.FromDate.HasValue)
                        query = query.Where(m => m.Timestamp >= options.FromDate.Value);
                    if (options.ToDate.HasValue)
                        query = query.Where(m => m.Timestamp <= options.ToDate.Value);
                }
                var count = await query.CountAsync();
                _cache.Set(countCacheKey, count, TimeSpan.FromMinutes(DefaultCacheMinutes));
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search count for channel {ChannelId}", channelId);
                return 0;
            }
        }

        public async Task InvalidateSearchCacheAsync(long channelId)
        {
            //do nothing for now
            _logger.LogInformation("Invalidating search cache for channel {ChannelId}", channelId);
            await Task.CompletedTask;
        }
        private string GenerateCacheKey(SearchOptions options)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"search:{options.ChannelId}:{options.QueryText.GetHashCode()}:{options.Page}:{options.PageSize}");
            if (options.FromDate.HasValue)
                keyBuilder.Append($":{options.FromDate.Value.Ticks}");
            if (options.ToDate.HasValue)
                keyBuilder.Append($":{options.ToDate.Value.Ticks}");
            if (!string.IsNullOrEmpty(options.SenderId))
            {
                if (long.TryParse(options.SenderId, out long senderId) && senderId > 0)
                {
                    keyBuilder.Append($":{options.SenderId}");
                }
            }
            keyBuilder.Append($":{options.SortBy}:{options.SortDescending}");
            if (!string.IsNullOrEmpty(options.CursorToken))
                keyBuilder.Append($":{options.CursorToken}");
            return keyBuilder.ToString();
        }

        private string SanitizeSearchQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return string.Empty;
            return query
                .Replace("&", " ")
                .Replace("|", " ")
                .Replace("!", " ")
                .Replace("(", " ")
                .Replace(")", " ")
                .Replace(":", " ")
                .Trim();
        }
        private string EncodeCursor(DateTimeOffset timestamp)
        {
            long ticks = timestamp.UtcTicks;
            var bytes = BitConverter.GetBytes(ticks);
            return Convert.ToBase64String(bytes);
        }
        private bool TryDecodeCursor(string cursor, out DateTimeOffset timestamp)
        {
            timestamp = default;
            try
            {
                var bytes = Convert.FromBase64String(cursor);
                var binary = BitConverter.ToInt64(bytes, 0);
                timestamp = new DateTimeOffset(binary, TimeSpan.Zero);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
