using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace iChat.Client.Services.UserServices.Chat
{
    public class ChatMessageCacheService
    {
        private readonly ConcurrentDictionary<long, SortedList<int, BucketDto>> _messageCache = new();
        private readonly ConcurrentDictionary<long, string> last_seen = new();
        private readonly ConcurrentDictionary<long, int> _latestBucketMap = new();
        private readonly JwtAuthHandler _http;
        public event Func<ChatMessageDtoSafe, Task>? OnMessageReceived;
        public event Func<EditMessageRt, Task>? OnMessageEdited;
        public event Func<DeleteMessageRt, Task>? OnMessageDeleted;
        public ChatMessageCacheService(JwtAuthHandler http)
        {
            _http = http;
        }
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
        public void ResigterOnMessageDeleted(Func<DeleteMessageRt, Task> onMessageDeleted)
        {
            OnMessageDeleted -= onMessageDeleted;
            OnMessageDeleted += onMessageDeleted;
        }
        public async Task HandleDeleteMessage(DeleteMessageRt delete)
        {
            var channelId = long.Parse(delete.ChannelId);
            var messageId = long.Parse(delete.MessageId);

            if (_messageCache.TryGetValue(channelId, out var buckets))
            {
               
                if (_latestBucketMap[channelId]<delete.BucketId)
                delete.BucketId= int.MaxValue;
                if (buckets.TryGetValue(delete.BucketId, out var bucket))
                {
                    var target = bucket.ChatMessageDtos.FirstOrDefault(m => long.Parse(m.Id) == messageId);
                    if (target != null)
                    {
                        target.isDeleted = true;
                        target.Content = string.Empty;
                        target.ContentMedia = string.Empty;

                        await OnMessageDeleted?.Invoke(delete);
                    }
                    else
                    {
                        Console.WriteLine($"Message {messageId} not found in bucket {delete.BucketId} for channel {channelId}");
                    }
                }
                else
                    Console.WriteLine($"Bucket {delete.BucketId} not found for channel {channelId}");
            }
        }
        public async Task HandleEditMessage(EditMessageRt rq)
        {
            var channelId = long.Parse(rq.ChannelId);
            if (_messageCache.TryGetValue(channelId, out var buckets))
            {
                if (buckets.TryGetValue(int.MaxValue, out var bucket))
                {
                    var target = bucket.ChatMessageDtos.FirstOrDefault(m => m.Id == rq.MessageId);
                    if (target != null && !target.isDeleted)
                    {
                        target.isEdited = true;
                        target.Content = rq.NewContent;
                        await OnMessageEdited?.Invoke(rq);
                    }
                }
            }
        }

        public async Task AddLatestMessage(ChatMessageDtoSafe message )
        {
            await OnMessageReceived.Invoke(message);
            string chatchannelId= message.ChannelId;
            var channelId = long.Parse(chatchannelId);
            if(_messageCache.TryGetValue(channelId,out var buckets))
            {
                var bucket = buckets[int.MaxValue];
                bucket.ChatMessageDtos.Add(message);
                var messageid= long.Parse(message.Id);
                if(long.Parse(bucket.LastSequence)< messageid)
                {
                    bucket.LastSequence = message.Id;
                }
                else
                if(long.Parse(bucket.FirstSequence) > messageid)
                {
                    bucket.FirstSequence = message.Id;
                }
                
            }
            else
            {

            }
        }
        public void UpdateLastSeen(string chatChannelId)
        {
            var channelId = long.Parse(chatChannelId);
            var currentchannel = _messageCache[channelId];
            var lastsequence= currentchannel[int.MaxValue].LastSequence;
            if (lastsequence == "")
            {
                lastsequence = currentchannel[currentchannel.Keys[^2]].LastSequence;
                if (lastsequence == "")
                    return;
            }
            last_seen[channelId] = lastsequence;
        }

        public async Task<(List<BucketDto> buckets, string loc)> GetLatestMessage(string chatChannelId)
        {
            var channelId = long.Parse(chatChannelId);
            if (!last_seen.TryGetValue(channelId, out var loc))
            {
                await LoadLatestBucketsFromServer(channelId);
                loc = _messageCache[channelId].Last().Value.LastSequence;
            }
            return (_messageCache[channelId].Values.ToList(), loc);
        }

        public async Task<List<BucketDto>> GetBucketsInRange(long chatChannelId, int startId, int endId)
        {
            var result = new List<BucketDto>();
            var list = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());

            for (int i = startId; i <= endId; i++)
            {
                if (!list.ContainsKey(i))
                {
                    await LoadPrevMessageBuckets(chatChannelId, i, endId);
                    break;
                }
            }

            for (int i = startId; i <= endId; i++)
            {
                if (list.TryGetValue(i, out var bucket))
                {
                    result.Add(bucket);
                }
            }

            return result;
        }

        public async Task<BucketDto?> GetBucketContainingMessage(long chatChannelId, string messageId)
        {
            var list = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());

            foreach (var bucket in list.Values)
            {
                if (string.Compare(bucket.FirstSequence, messageId) <= 0 && string.Compare(bucket.LastSequence, messageId) >= 0)
                {
                    return bucket;
                }
            }

            var response = await _http.SendAuthAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{chatChannelId}/messagejump?id={messageId}"));
            if (response.IsSuccessStatusCode)
            {
                var bucket = await response.Content.ReadFromJsonAsync<BucketDto>();
                if (bucket != null)
                {
                    list[bucket.BucketId] = bucket;
                    return bucket;
                }
            }

            return null;
        }

        public async Task<BucketDto?> GetPreviousBucket(string ChannelId, int currentBucketId)
        {
            var chatChannelId = long.Parse(ChannelId);
            var list = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());
            int targetId = currentBucketId - 1;
            if (targetId < 0) return null;

            if (!list.TryGetValue(targetId, out var bucket))
            {
                await TryLoadBucket(chatChannelId, targetId, list);
                list.TryGetValue(targetId, out bucket);
            }

            return bucket;
        }

        public async Task<BucketDto?> GetNewerBucket(long chatChannelId, int currentBucketId)
        {
            var list = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());
            int targetId = currentBucketId >= int.MaxValue ? int.MaxValue : currentBucketId + 1;

            if (_latestBucketMap.TryGetValue(chatChannelId, out var latest) && targetId >= latest)
            {
                targetId = int.MaxValue;
            }

            if (!list.TryGetValue(targetId, out var bucket))
            {
                await TryLoadBucket(chatChannelId, targetId, list);
                list.TryGetValue(targetId, out bucket);
            }

            return bucket;
        }

        private async Task LoadPrevMessageBuckets(long chatChannelId, int startId, int endId)
        {
            if (startId < 0) startId = 0;

            var limit = Math.Min(endId - startId + 1, 3);
            var response = await _http.SendAuthAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{chatChannelId}/buckets?limit={limit}&endid={endId}"));

            if (response.IsSuccessStatusCode)
            {
                var buckets = await response.Content.ReadFromJsonAsync<List<BucketDto>>();
                if (buckets == null || buckets.Count == 0) return;

                var list = _messageCache.GetOrAdd(chatChannelId, _ => new SortedList<int, BucketDto>());
                foreach (var bucket in buckets)
                {
                    list[bucket.BucketId] = bucket;
                }
            }
            else
            {
                throw new HttpRequestException("Failed to load previous messages", null, response.StatusCode);
            }
        }

        private async Task LoadLatestBucketsFromServer(long chatChannelId)
        {
            var response = await _http.SendAuthAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{chatChannelId}/latest"));

                if (response.IsSuccessStatusCode)
            {
                var buckets = await response.Content.ReadFromJsonAsync<List<BucketDto>>();
                var list = new SortedList<int, BucketDto>();

                foreach (var bucket in buckets)
                {
                    list[bucket.BucketId] = bucket;
                }

                switch(buckets.Count)
                {
                    case 0:
                        _latestBucketMap[chatChannelId] = 0;
                        break;
                    case 1:
                        _latestBucketMap[chatChannelId] =0;
                        break;
                    default:
                        _latestBucketMap[chatChannelId] = buckets[^2].BucketId;
                        break;
                }
                _messageCache[chatChannelId] = list;
            }
            else
            {
                throw new HttpRequestException("Failed to load latest messages", null, response.StatusCode);
            }
        }

        private async Task TryLoadBucket(long chatChannelId, int bucketId, SortedList<int, BucketDto> list)
        {
            var response = await _http.SendAuthAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/ChatChannel/{chatChannelId}/bucketsingle?id={bucketId}"));
            if (response.IsSuccessStatusCode)
            {
                var bucket = await response.Content.ReadFromJsonAsync<BucketDto>();
                if (bucket != null)
                {
                    list[bucket.BucketId] = bucket;
                }
            }
        }
    }
}