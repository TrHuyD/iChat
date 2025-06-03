using System.Text.Json;
using System.Text.Json.Serialization;
using TG.Blazor.IndexedDB;
using iChat.DTOs.Users.Messages;
using Microsoft.JSInterop;

namespace iChat.Client.Services.UserServices.ChatService
{
    public class MessageStorageService : IAsyncDisposable
    {
        private readonly IndexedDBManager _dbManager;
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<MessageStorageService>? _dotNetRef;
        private bool ranOnce = false;

        public MessageStorageService(IndexedDBManager dbManager, IJSRuntime jsRuntime)
        {
            _dbManager = dbManager;
            _jsRuntime = jsRuntime;
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        public async Task InitializeAsync()
        {
            if (ranOnce)
                return;

            ranOnce = true;
            await _dbManager.OpenDb();

            
          // await _jsRuntime.InvokeVoidAsync("registerMessageStorageInterop", _dotNetRef);
        }

        public async Task StoreMessageAsync(string roomId, ChatMessageDto message)
        {
            var record = new StoreRecord<IndexedMessage>
            {
                Storename = "Messages",
                Data = new IndexedMessage
                {
                    Id = $"{roomId}_{message.Id}",
                    RoomId = roomId,
                    MessageId = message.Id,
                    Content = message.Content,
                    ContentMedia = message.ContentMedia,
                    MessageType = message.MessageType,
                    CreatedAt = message.CreatedAt.UtcDateTime,
                    SenderId = message.SenderId
                }
            };

            await _dbManager.AddRecord(record);
        }

        public async Task StoreMessagesAsync(string roomId, List<ChatMessageDto> messages)
        {
            var tasks = messages.Select(message => StoreMessageAsync(roomId, message));
            await Task.WhenAll(tasks);
        }

        public async Task ClearAllMessagesAsync()
        {
            try
            {
                await _dbManager.OpenDb();
                await _dbManager.ClearStore("Messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing all messages: {ex.Message}");
            }
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(string roomId, int limit = 100)
        {
            try
            {
                var messages = await _dbManager.GetAllRecordsByIndex<string, IndexedMessage>(
                    new StoreIndexQuery<string>
                    {
                        Storename = "Messages",
                        IndexName = "roomId",
                        QueryValue = roomId
                    });

                return messages?
                    .OrderBy(m => m.CreatedAt)
                    .TakeLast(limit)
                    .Select(m => new ChatMessageDto
                    {
                        Id = m.MessageId,
                        Content = m.Content,
                        ContentMedia = m.ContentMedia,
                        MessageType = m.MessageType,
                        CreatedAt = m.CreatedAt.ToLocalTime(),
                        SenderId = m.SenderId,
                        RoomId = long.Parse(roomId)
                    })
                    .ToList() ?? new List<ChatMessageDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting messages: {ex.Message}");
                return new List<ChatMessageDto>();
            }
        }

        public async Task ClearMessagesAsync(string roomId)
        {
            try
            {
                var messages = await _dbManager.GetAllRecordsByIndex<string, IndexedMessage>(
                    new StoreIndexQuery<string>
                    {
                        Storename = "Messages",
                        IndexName = "roomId",
                        QueryValue = roomId
                    });

                if (messages == null) return;

                var tasks = messages.Select(message =>
                    _dbManager.DeleteRecord<string>("Messages", message.Id));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing messages: {ex.Message}");
            }
        }

        public async Task<int> GetMessageCountAsync(string roomId)
        {
            try
            {
                var messages = await _dbManager.GetAllRecordsByIndex<string, IndexedMessage>(
                    new StoreIndexQuery<string>
                    {
                        Storename = "Messages",
                        IndexName = "roomId",
                        QueryValue = roomId
                    });

                return messages?.Count ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting messages: {ex.Message}");
                return 0;
            }
        }

        [JSInvokable]
        public async Task StoreMessageAsyncFromJs(string roomId, JsonElement messageJson)
        {
            var message = JsonSerializer.Deserialize<ChatMessageDto>(messageJson.ToString()!);
            if (message != null)
            {
                await StoreMessageAsync(roomId, message);
            }
        }

        [JSInvokable]
        public async Task StoreMessagesAsyncFromJs(string roomId, JsonElement messagesJson)
        {
            var messages = JsonSerializer.Deserialize<List<ChatMessageDto>>(messagesJson.ToString()!);
            if (messages != null)
            {
                await StoreMessagesAsync(roomId, messages);
            }
        }

        [JSInvokable]
        public async Task DeleteMessagesInRoomAsyncFromJs(string roomId)
        {
            await ClearMessagesAsync(roomId);
        }

        public async ValueTask DisposeAsync()
        {
            _dotNetRef?.Dispose();
        }

        public class IndexedMessage
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("roomId")]
            public string RoomId { get; set; }

            [JsonPropertyName("messageId")]
            public long MessageId { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("contentMedia")]
            public string ContentMedia { get; set; }

            [JsonPropertyName("messageType")]
            public int MessageType { get; set; }

            [JsonPropertyName("createdAt")]
            public DateTime CreatedAt { get; set; }

            [JsonPropertyName("senderId")]
            public long SenderId { get; set; }
        }
    }
}
