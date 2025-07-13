using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace iChat.Client.Services.UserServices.Chat
{
    public class ChatMessageCacheService
    {
        private readonly ConcurrentDictionary<long, SortedList<int, BucketDto>> _messageCache = new();
        private readonly ConcurrentDictionary<long, string> _lastSeen = new();
        private readonly ConcurrentDictionary<long, int> _latestBucketMap = new();
        private readonly JwtAuthHandler _httpClient;

        // Events
        public event Func<ChatMessageDtoSafe, Task>? OnMessageReceived;
        public event Func<EditMessageRt, Task>? OnMessageEdited;
        public event Func<DeleteMessageRt, Task>? OnMessageDeleted;

        public ChatMessageCacheService(JwtAuthHandler httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        #region Event Registration
        public void RegisterOnMessageReceived(Func<ChatMessageDtoSafe, Task> onMessageReceived)
        {
            OnMessageReceived -= onMessageReceived;
            OnMessageReceived += onMessageReceived;
        }

        public void RegisterOnMessageEdited(Func<EditMessageRt, Task> onMessageEdited)
        {
            OnMessageEdited -= onMessageEdited;
            OnMessageEdited += onMessageEdited;
        }

        public void RegisterOnMessageDeleted(Func<DeleteMessageRt, Task> onMessageDeleted)
        {
            OnMessageDeleted -= onMessageDeleted;
            OnMessageDeleted += onMessageDeleted;
        }
        #endregion

        #region Message Operations
        public async Task HandleDeleteMessage(DeleteMessageRt delete)
        {
            if (delete == null) throw new ArgumentNullException(nameof(delete));

            var channelId = long.Parse(delete.ChannelId);
            var messageId = long.Parse(delete.MessageId);

            var target = FindSpecificMessageWithFallback(channelId, delete.BucketId, messageId);
            if (target != null)
            {
                target.isDeleted = true;
                target.Content = string.Empty;
                target.ContentMedia = string.Empty;

                if (OnMessageDeleted != null)
                    await OnMessageDeleted.Invoke(delete);
            }
            else
            {
                Console.WriteLine($"Message {messageId} not found in bucket {delete.BucketId} for channel {channelId}");
            }
        }

        public async Task HandleEditMessage(EditMessageRt editRequest)
        {
            if (editRequest == null) throw new ArgumentNullException(nameof(editRequest));

            var channelId = long.Parse(editRequest.ChannelId);
            var messageId = long.Parse(editRequest.MessageId);

            var target = FindSpecificMessageWithFallback(channelId, editRequest.BucketId, messageId);
            if (target != null && !target.isDeleted)
            {
                target.isEdited = true;
                target.Content = editRequest.NewContent;

                if (OnMessageEdited != null)
                    await OnMessageEdited.Invoke(editRequest);
            }
        }

        public async Task AddLatestMessage(ChatMessageDtoSafe message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (OnMessageReceived != null)
                await OnMessageReceived.Invoke(message);

            var channelId = long.Parse(message.ChannelId);
            var messageId = long.Parse(message.Id);

            if (_messageCache.TryGetValue(channelId, out var buckets))
            {
                var latestBucket = buckets[int.MaxValue];
                latestBucket.ChatMessageDtos.Add(message);

                UpdateBucketSequence(latestBucket, message.Id, messageId);
            }
            else
            {
                // Handle case where channel doesn't exist in cache
                Console.WriteLine($"Channel {channelId} not found in cache when adding latest message");
            }
        }

        public void UpdateLastSeen(string chatChannelId)
        {
            if (string.IsNullOrEmpty(chatChannelId)) return;

            var channelId = long.Parse(chatChannelId);

            if (!_messageCache.TryGetValue(channelId, out var buckets)) return;

            var lastSequence = GetLastSequenceFromBuckets(buckets);
            if (!string.IsNullOrEmpty(lastSequence))
            {
                _lastSeen[channelId] = lastSequence;
            }
        }
        #endregion

        #region Data Retrieval
        public async Task<(List<BucketDto> buckets, string location)> GetLatestMessage(string chatChannelId)
        {
            if (string.IsNullOrEmpty(chatChannelId))
                throw new ArgumentException("Channel ID cannot be null or empty", nameof(chatChannelId));

            var channelId = long.Parse(chatChannelId);

            if (!_lastSeen.TryGetValue(channelId, out var location))
            {
                await LoadLatestBucketsFromServer(channelId);
                location = _messageCache[channelId].Last().Value.LastSequence;
            }

            return (_messageCache[channelId].Values.ToList(), location);
        }

        public async Task<List<BucketDto>> GetBucketsInRange(long chatChannelId, int startId, int endId)
        {
            var result = new List<BucketDto>();
            var bucketList = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());

            // Load missing buckets
            await EnsureBucketsLoaded(chatChannelId, startId, endId, bucketList);

            // Collect available buckets in range
            for (int i = startId; i <= endId; i++)
            {
                if (bucketList.TryGetValue(i, out var bucket))
                {
                    result.Add(bucket);
                }
            }

            return result;
        }

        public async Task<BucketDto?> GetBucketContainingMessage(long chatChannelId, string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) return null;

            var bucketList = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());

            // Check existing buckets first
            foreach (var bucket in bucketList.Values)
            {
                if (IsMessageInBucket(bucket, messageId))
                {
                    return bucket;
                }
            }

            // Load from server if not found locally
            return await LoadBucketFromServer(chatChannelId, messageId, bucketList);
        }

        public async Task<BucketDto?> GetPreviousBucket(string channelId, int currentBucketId)
        {
            if (string.IsNullOrEmpty(channelId)) return null;

            var chatChannelId = long.Parse(channelId);
            var bucketList = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());

            int targetId = currentBucketId - 1;
            if (targetId < 0) return null;

            if (!bucketList.TryGetValue(targetId, out var bucket))
            {
                await TryLoadBucket(chatChannelId, targetId, bucketList);
                bucketList.TryGetValue(targetId, out bucket);
            }

            return bucket;
        }

        public async Task<BucketDto?> GetNewerBucket(long chatChannelId, int currentBucketId)
        {
            var bucketList = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());
            int targetId = DetermineTargetBucketId(chatChannelId, currentBucketId);

            if (!bucketList.TryGetValue(targetId, out var bucket))
            {
                await TryLoadBucket(chatChannelId, targetId, bucketList);
                bucketList.TryGetValue(targetId, out bucket);
            }

            return bucket;
        }
        #endregion

        #region Private Helper Methods
        private ChatMessageDtoSafe? FindMessageInBucket(BucketDto bucket, long messageId)
        {
            if (bucket?.ChatMessageDtos == null) return null;

            if (!(bucket.FirstSequenceLong <= messageId && bucket.LastSequenceLong >= messageId))
                return null;

            return bucket.ChatMessageDtos.FirstOrDefault(m => m.IdLong == messageId);
        }

        private ChatMessageDtoSafe? FindSpecificMessageWithFallback(long channelId, int bucketId, long messageId)
        {
            if (!_messageCache.TryGetValue(channelId, out var buckets))
            {
                Console.WriteLine($"Channel {channelId} not found in cache.");
                return null;
            }

            // Adjust bucket ID if it's beyond the latest
            if (_latestBucketMap.TryGetValue(channelId, out var latestBucket) && latestBucket < bucketId)
            {
                bucketId = int.MaxValue;
            }

            // Try specific bucket first
            if (buckets.TryGetValue(bucketId, out var bucket))
            {
                var message = FindMessageInBucket(bucket, messageId);
                if (message != null) return message;
            }
            else
            {
                Console.WriteLine($"Bucket {bucketId} not found for channel {channelId} in cache, trying latest bucket.");
            }

            // Fallback to latest bucket
            if (buckets.TryGetValue(int.MaxValue, out var latestBucketData))
            {
                var message = FindMessageInBucket(latestBucketData, messageId);
                if (message != null) return message;
            }

            Console.WriteLine($"Message {messageId} not found in bucket {bucketId} or latest bucket for channel {channelId}.");
            return null;
        }

        private void UpdateBucketSequence(BucketDto bucket, string messageId, long messageIdLong)
        {
            if (long.Parse(bucket.LastSequence) < messageIdLong)
            {
                bucket.LastSequence = messageId;
            }
            else if (long.Parse(bucket.FirstSequence) > messageIdLong)
            {
                bucket.FirstSequence = messageId;
            }
        }

        private string GetLastSequenceFromBuckets(SortedList<int, BucketDto> buckets)
        {
            var latestBucket = buckets[int.MaxValue];
            var lastSequence = latestBucket.LastSequence;

            if (string.IsNullOrEmpty(lastSequence) && buckets.Count > 1)
            {
                // Try second-to-last bucket
                lastSequence = buckets[buckets.Keys[^2]].LastSequence;
            }

            return lastSequence;
        }

        private async Task EnsureBucketsLoaded(long chatChannelId, int startId, int endId, SortedList<int, BucketDto> bucketList)
        {
            for (int i = startId; i <= endId; i++)
            {
                if (!bucketList.ContainsKey(i))
                {
                    await LoadPrevMessageBuckets(chatChannelId, i, endId);
                    break;
                }
            }
        }

        private bool IsMessageInBucket(BucketDto bucket, string messageId)
        {
            return string.Compare(bucket.FirstSequence, messageId, StringComparison.Ordinal) <= 0 &&
                   string.Compare(bucket.LastSequence, messageId, StringComparison.Ordinal) >= 0;
        }

        private async Task<BucketDto?> LoadBucketFromServer(long chatChannelId, string messageId, SortedList<int, BucketDto> bucketList)
        {
            try
            {
                var response = await _httpClient.SendAuthAsync(
                    new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{chatChannelId}/messagejump?id={messageId}"));

                if (response.IsSuccessStatusCode)
                {
                    var bucket = await response.Content.ReadFromJsonAsync<BucketDto>();
                    if (bucket != null)
                    {
                        bucketList[bucket.BucketId] = bucket;
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

        private int DetermineTargetBucketId(long chatChannelId, int currentBucketId)
        {
            int targetId = currentBucketId >= int.MaxValue ? int.MaxValue : currentBucketId + 1;

            if (_latestBucketMap.TryGetValue(chatChannelId, out var latest) && targetId >= latest)
            {
                targetId = int.MaxValue;
            }

            return targetId;
        }
        #endregion

        #region Server Communication
        private async Task LoadPrevMessageBuckets(long chatChannelId, int startId, int endId)
        {
            if (startId < 0) startId = 0;

            var limit = Math.Min(endId - startId + 1, 3);

            try
            {
                var response = await _httpClient.SendAuthAsync(
                    new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{chatChannelId}/buckets?limit={limit}&endid={endId}"));

                if (response.IsSuccessStatusCode)
                {
                    var buckets = await response.Content.ReadFromJsonAsync<List<BucketDto>>();
                    if (buckets?.Count > 0)
                    {
                        var bucketList = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());
                        foreach (var bucket in buckets)
                        {
                            bucketList[bucket.BucketId] = bucket;
                        }
                    }
                }
                else
                {
                    throw new HttpRequestException("Failed to load previous messages", null, response.StatusCode);
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error loading previous messages: {ex.Message}", ex);
            }
        }

        private async Task LoadLatestBucketsFromServer(long chatChannelId)
        {
            try
            {
                var response = await _httpClient.SendAuthAsync(
                    new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{chatChannelId}/latest"));

                if (response.IsSuccessStatusCode)
                {
                    var buckets = await response.Content.ReadFromJsonAsync<List<BucketDto>>();
                    if (buckets != null)
                    {
                        var bucketList = new SortedList<int, BucketDto>();
                        foreach (var bucket in buckets)
                        {
                            bucketList[bucket.BucketId] = bucket;
                        }

                        UpdateLatestBucketMap(chatChannelId, buckets);
                        _messageCache[chatChannelId] = bucketList;
                    }
                }
                else
                {
                    throw new HttpRequestException("Failed to load latest messages", null, response.StatusCode);
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error loading latest messages: {ex.Message}", ex);
            }
        }
        private void UpdateLatestBucketMap(long chatChannelId, List<BucketDto> buckets)
        {
            _latestBucketMap[chatChannelId] = buckets.Count switch
            {
                0 or 1 => 0,
                _ => buckets[^2].BucketId
            };
        }
        private async Task TryLoadBucket(long chatChannelId, int bucketId, SortedList<int, BucketDto> bucketList)
        {
            try
            {
                var response = await _httpClient.SendAuthAsync(
                    new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{chatChannelId}/bucketsingle?id={bucketId}"));

                if (response.IsSuccessStatusCode)
                {
                    var bucket = await response.Content.ReadFromJsonAsync<BucketDto>();
                    if (bucket != null)
                    {
                        bucketList[bucket.BucketId] = bucket;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load bucket {bucketId} for channel {chatChannelId}: {ex.Message}");
            }
        }
        #endregion
    }
}