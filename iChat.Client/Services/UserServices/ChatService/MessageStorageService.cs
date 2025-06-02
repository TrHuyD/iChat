using System.Text.Json;
using System.Text.Json.Serialization;
using TG.Blazor.IndexedDB;
using iChat.DTOs.Users.Messages;

namespace iChat.Client.Services.UserServices.ChatService
{
    public class MessageStorageService
    {
        private readonly IndexedDBManager _dbManager;
        //private readonly UserStateService _user;
        
        public MessageStorageService(IndexedDBManager dbManager)
        {
            _dbManager = dbManager;
            
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

                foreach (var message in messages)
                {
                    // Correct way to delete a record by its primary key
                    await _dbManager.DeleteRecord<string>("Messages", message.Id);
                }
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
        private bool ranOnce = false;
        public async Task Initial()
        {
            if (ranOnce)
                return;
            ranOnce = true;
            await _dbManager.OpenDb();
        }

        public class IndexedMessage
        {
            [System.Text.Json.Serialization.JsonPropertyName("id")]
            public string Id { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("roomId")]
            public string RoomId { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("messageId")]
            public long MessageId { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("content")]
            public string Content { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("contentMedia")]
            public string ContentMedia { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("messageType")]
            public int MessageType { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
            public DateTime CreatedAt { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("senderId")]
            public long SenderId { get; set; }
        }
    }
}