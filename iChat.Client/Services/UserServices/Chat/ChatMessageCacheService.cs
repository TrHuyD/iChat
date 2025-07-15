using iChat.Client.Data;
using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace iChat.Client.Services.UserServices.Chat
{
    public class ChatMessageCacheService
    {
        private readonly ConcurrentDictionary<long, SortedList<int, MessageBucket>> _messageCache = new();
        private readonly ConcurrentDictionary<long, long> _lastSeen = new();
        private readonly ConcurrentDictionary<long, int> _latestBucketMap = new();
        private readonly JwtAuthHandler _httpClient;

        public event Func<ChatMessageDto, Task>? OnMessageReceived;
        public event Func<EditMessageRt, Task>? OnMessageEdited;
        public event Func<DeleteMessageRt, Task>? OnMessageDeleted;

        public ChatMessageCacheService(JwtAuthHandler httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        #region Event Registration
        public void RegisterOnMessageReceived(Func<ChatMessageDto, Task> callback) => OnMessageReceived = callback;
        public void RegisterOnMessageEdited(Func<EditMessageRt, Task> callback) => OnMessageEdited = callback;
        public void RegisterOnMessageDeleted(Func<DeleteMessageRt, Task> callback) => OnMessageDeleted = callback;
        #endregion

        #region Message Operations
        public async Task HandleDeleteMessage(DeleteMessageRt delete)
        {
            var channelId = long.Parse(delete.ChannelId);
            var messageId = long.Parse(delete.MessageId);

            var target = FindSpecificMessageWithFallback(channelId, delete.BucketId, messageId);
            if (target != null)
            {
                target.IsDeleted = true;
                target.Content = string.Empty;
                target.ContentMedia =null;
                if (OnMessageDeleted != null) await OnMessageDeleted(delete);
            }
        }

        public async Task HandleEditMessage(EditMessageRt edit)
        {
            var channelId = long.Parse(edit.ChannelId);
            var messageId = long.Parse(edit.MessageId);

            var target = FindSpecificMessageWithFallback(channelId, edit.BucketId, messageId);
            if (target != null && !target.IsDeleted)
            {
                target.IsEdited = true;
                target.Content = edit.NewContent;
                if (OnMessageEdited != null) await OnMessageEdited(edit);
            }
        }

        public async Task AddLatestMessage(ChatMessageDto message)
        {
            if (_messageCache.TryGetValue(message.ChannelId, out var buckets) &&
                buckets.TryGetValue(int.MaxValue, out var latestBucket))
            {
                latestBucket.ChatMessageDtos.Add(message);
                UpdateBucketSequence(latestBucket, message.Id);
                if (OnMessageReceived != null) await OnMessageReceived(message);
            }
        }

        public void UpdateLastSeen(string channelId)
        {
            if (!long.TryParse(channelId, out var id)) return;
            if (_messageCache.TryGetValue(id, out var buckets))
                _lastSeen[id] = GetLastSequenceFromBuckets(buckets);
        }
        #endregion

        #region Data Retrieval
        public async Task<(List<MessageBucket>, long)> GetLatestMessage(string channelId)
        {
            var id = long.Parse(channelId);

            if (!_lastSeen.TryGetValue(id, out var sequence))
            {
                await LoadLatestBucketsFromServer(id);
                sequence = _messageCache[id].Last().Value.LastSequence;
            }

            return (_messageCache[id].Values.ToList(), sequence);
        }

        public async Task<List<MessageBucket>> GetBucketsInRange(long channelId, int startId, int endId)
        {
            var buckets = _messageCache.GetOrAdd(channelId, _ => new());
            await EnsureBucketsLoaded(channelId, startId, endId, buckets);

            var list = new List<MessageBucket>();
            for (int i = startId; i <= endId; i++)
                if (buckets.TryGetValue(i, out var bucket))
                    list.Add(bucket);

            return list;
        }

        public async Task<MessageBucket?> GetBucketContainingMessage(long channelId, string messageId)
        {
            var buckets = _messageCache.GetOrAdd(channelId, _ => new());

            foreach (var bucket in buckets.Values)
            {
                if (IsMessageInBucket(bucket, messageId))
                    return bucket;
            }

            return await LoadBucketFromServer(channelId, messageId, buckets);
        }

        public async Task<MessageBucket?> GetPreviousBucket(string channelId, int currentBucketId)
        {
            var id = long.Parse(channelId);
            var target = currentBucketId - 1;
            if (target < 0) return null;

            var buckets = _messageCache.GetOrAdd(id, _ => new());
            if (!buckets.TryGetValue(target, out var bucket))
            {
                await TryLoadBucket(id, target, buckets);
                buckets.TryGetValue(target, out bucket);
            }

            return bucket;
        }

        public async Task<MessageBucket?> GetNewerBucket(long channelId, int currentBucketId)
        {
            var buckets = _messageCache.GetOrAdd(channelId, _ => new());
            var targetId = DetermineTargetBucketId(channelId, currentBucketId);

            if (!buckets.TryGetValue(targetId, out var bucket))
            {
                await TryLoadBucket(channelId, targetId, buckets);
                buckets.TryGetValue(targetId, out bucket);
            }

            return bucket;
        }
        #endregion

        #region Private Helpers
        private ChatMessageDto? FindSpecificMessageWithFallback(long channelId, int bucketId, long messageId)
        {
            if (!_messageCache.TryGetValue(channelId, out var buckets)) return null;

            if (_latestBucketMap.TryGetValue(channelId, out var latest) && bucketId > latest)
                bucketId = int.MaxValue;

            return buckets.TryGetValue(bucketId, out var bucket) && IsInRange(bucket, messageId)
                ? bucket.ChatMessageDtos.FirstOrDefault(m => m.Id == messageId)
                : buckets.TryGetValue(int.MaxValue, out var latestBucket)
                    ? latestBucket.ChatMessageDtos.FirstOrDefault(m => m.Id == messageId)
                    : null;
        }

        private bool IsMessageInBucket(MessageBucket bucket, string messageId)
            => string.Compare(bucket.FirstSequence.ToString(), messageId) <= 0 &&
               string.Compare(bucket.LastSequence.ToString(), messageId) >= 0;

        private bool IsInRange(MessageBucket bucket, long messageId)
            => bucket.FirstSequence <= messageId && bucket.LastSequence >= messageId;

        private void UpdateBucketSequence(MessageBucket bucket, long messageId)
        {
            if (bucket.LastSequence < messageId)
                bucket.LastSequence = messageId;
        }

        private long GetLastSequenceFromBuckets(SortedList<int, MessageBucket> buckets)
        {
            if (buckets.TryGetValue(int.MaxValue, out var latest))
                return latest.LastSequence;

            return buckets.Count > 0 ? buckets.Values.Last().LastSequence : 0;
        }

        private async Task EnsureBucketsLoaded(long channelId, int startId, int endId, SortedList<int, MessageBucket> list)
        {
            for (int i = startId; i <= endId; i++)
            {
                if (!list.ContainsKey(i))
                {
                    await LoadPrevMessageBuckets(channelId, i, endId);
                    break;
                }
            }
        }

        private int DetermineTargetBucketId(long channelId, int current)
        {
            var next = current >= int.MaxValue ? int.MaxValue : current + 1;
            if (_latestBucketMap.TryGetValue(channelId, out var latest) && next >= latest)
                return int.MaxValue;
            return next;
        }
        #endregion

        #region Server Communication
        private async Task LoadPrevMessageBuckets(long channelId, int startId, int endId)
        {
            var limit = Math.Min(endId - startId + 1, 3);
            var url = $"/api/ChatChannel/{channelId}/buckets?limit={limit}&endid={endId}";

            var response = await _httpClient.SendAuthAsync(new HttpRequestMessage(HttpMethod.Get, url));
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Failed to load previous messages", null, response.StatusCode);

            var bucketDtos = await response.Content.ReadFromJsonAsync<List<BucketDto>>();
            if (bucketDtos == null) return;

            var buckets = _messageCache.GetOrAdd(channelId, _ => new());
            foreach (var dto in bucketDtos)
            {
                var bucket = new MessageBucket(dto);
                buckets[bucket.BucketId] = bucket;
            }
        }

        private async Task LoadLatestBucketsFromServer(long channelId)
        {
            var response = await _httpClient.SendAuthAsync(
                new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{channelId}/latest"));

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Failed to load latest messages", null, response.StatusCode);

            var dtos = await response.Content.ReadFromJsonAsync<List<BucketDto>>();
            if (dtos == null) return;

            var list = new SortedList<int, MessageBucket>();
            foreach (var dto in dtos)
            {
                var bucket = new MessageBucket(dto);
                list[bucket.BucketId] = bucket;
            }

            UpdateLatestBucketMap(channelId, list);
            _messageCache[channelId] = list;
        }

        private void UpdateLatestBucketMap(long channelId, SortedList<int, MessageBucket> list)
        {
            _latestBucketMap[channelId] = list.Count switch
            {
                0 or 1 => 0,
                _ => list.Values[^2].BucketId
            };
        }

        private async Task TryLoadBucket(long channelId, int bucketId, SortedList<int, MessageBucket> list)
        {
            try
            {
                var response = await _httpClient.SendAuthAsync(
                    new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{channelId}/bucketsingle?id={bucketId}"));

                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<BucketDto>();
                    if (dto != null)
                        list[dto.BucketId] = new MessageBucket(dto);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading bucket {bucketId}: {ex.Message}");
            }
        }

        private async Task<MessageBucket?> LoadBucketFromServer(long channelId, string messageId, SortedList<int, MessageBucket> list)
        {
            try
            {
                var response = await _httpClient.SendAuthAsync(
                    new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{channelId}/messagejump?id={messageId}"));

                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<BucketDto>();
                    if (dto != null)
                    {
                        var bucket = new MessageBucket(dto);
                        list[bucket.BucketId] = bucket;
                        return bucket;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to load bucket for message {messageId}: {ex.Message}");
            }

            return null;
        }
        #endregion
    }
}
