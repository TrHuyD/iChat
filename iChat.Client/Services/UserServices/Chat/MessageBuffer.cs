//using iChat.DTOs.Users.Messages;

//namespace iChat.Client.Services.UserServices.Chat
//{
//    public class MessageBuffer
//    {
//        private List<ChatMessageDtoWithLongId> _messages;

//        public MessageBuffer()
//        {
//            _messages = new List<ChatMessageDtoWithLongId>();
//        }

//        public int Count => _messages.Count;

//        public IReadOnlyList<ChatMessageDtoWithLongId> Messages => _messages.AsReadOnly();

//        /// <summary>
//        /// Appends a single message to the end of the buffer
//        /// </summary>
//        public void AppendMessage(ChatMessageDtoSafe message)
//        {
//            var messageWithLongId = new ChatMessageDtoWithLongId(message);
//            _messages.Add(messageWithLongId);
//            SortMessages();
//        }

//        /// <summary>
//        /// Takes out a chunk of messages from the buffer
//        /// </summary>
//        public List<ChatMessageDtoWithLongId> TakeChunk(int count)
//        {
//            if (count <= 0 || _messages.Count == 0)
//                return new List<ChatMessageDtoWithLongId>();

//            int takeCount = Math.Min(count, _messages.Count);
//            var chunk = _messages.Take(takeCount).ToList();
//            _messages.RemoveRange(0, takeCount);

//            return chunk;
//        }

//        /// <summary>
//        /// Inserts a list of messages at the specified index (0 = start)
//        /// Handles gap message creation and cleanup automatically
//        /// </summary>
//        public List<ChatMessageDtoWithLongId> InsertMessages(List<ChatMessageDtoSafe> newMessages, int index = 0)
//        {
//            if (newMessages == null || newMessages.Count == 0)
//                return _messages.ToList();

//            var newMessagesWithLongId = newMessages.Select(m => new ChatMessageDtoWithLongId(m)).ToList();

//            if (index == 0 && _messages.Count == 0)
//            {
//                // First insertion - add messages and create gap message at start
//                _messages.AddRange(newMessagesWithLongId);
//                SortMessages();
//                CreateGapMessageAtStart();
//            }
//            else if (index == 0)
//            {
//                // Inserting at start - this is likely filling a gap
//                HandleGapFilling(newMessagesWithLongId);
//            }
//            else
//            {
//                // Insert at specified index
//                _messages.InsertRange(index, newMessagesWithLongId);
//                SortMessages();
//            }

//            return _messages.ToList();
//        }

//        private void HandleGapFilling(List<ChatMessageDtoWithLongId> newMessages)
//        {
//            // Remove existing gap message at start if it exists
//            if (_messages.Count > 0 && _messages[0].IsGapMessage)
//            {
//                _messages.RemoveAt(0);
//            }

//            // Add new messages
//            _messages.InsertRange(0, newMessages);
//            SortMessages();

//            // Create new gap message at start
//            CreateGapMessageAtStart();
//        }

//        private void CreateGapMessageAtStart()
//        {
//            if (_messages.Count == 0) return;

//            var firstMessage = _messages.OrderBy(m => m.LongId).First();
//            var gapMessage = new ChatMessageDtoWithLongId
//            {
//                Id = (firstMessage.LongId - 1).ToString(),
//                Content = firstMessage.Id, // Gap message content is the base message's ID
//                ContentMedia = string.Empty,
//                MessageType = firstMessage.MessageType,
//                CreatedAt = firstMessage.CreatedAt.AddMilliseconds(-1),
//                SenderId = firstMessage.SenderId,
//                ChannelId = firstMessage.ChannelId,
//                LongId = firstMessage.LongId - 1,
//                IsGapMessage = true
//            };

//            _messages.Insert(0, gapMessage);
//            SortMessages();
//        }

//        private void SortMessages()
//        {
//            _messages = _messages.OrderBy(m => m.LongId).ToList();
//        }

//        /// <summary>
//        /// Gets all messages including gap messages
//        /// </summary>
//        public List<ChatMessageDtoWithLongId> GetAllMessages()
//        {
//            return _messages.ToList();
//        }

//        /// <summary>
//        /// Gets only real messages (excludes gap messages)
//        /// </summary>
//        public List<ChatMessageDtoWithLongId> GetRealMessages()
//        {
//            return _messages.Where(m => !m.IsGapMessage).ToList();
//        }

//        /// <summary>
//        /// Clears all messages from the buffer
//        /// </summary>
//        public void Clear()
//        {
//            _messages.Clear();
//        }

//        /// <summary>
//        /// Gets the current gap message ID if it exists
//        /// </summary>
//        public string GetCurrentGapMessageId()
//        {
//            var gapMessage = _messages.FirstOrDefault(m => m.IsGapMessage);
//            return gapMessage?.Content; // Gap message content contains the base message ID
//        }
//    }
//    public class ChatMessageDtoWithLongId : ChatMessageDtoSafe
//    {
//        public long LongId { get; set; }
//        public bool IsGapMessage { get; set; } = false;

//        public ChatMessageDtoWithLongId()
//        {
//        }

//        public ChatMessageDtoWithLongId(ChatMessageDtoSafe baseMessage)
//        {
//            Id = baseMessage.Id;
//            Content = baseMessage.Content;
//            ContentMedia = baseMessage.ContentMedia;
//            MessageType = baseMessage.MessageType;
//            CreatedAt = baseMessage.CreatedAt;
//            SenderId = baseMessage.SenderId;
//            ChannelId = baseMessage.ChannelId;
//            LongId = ParseSnowflakeId(baseMessage.Id);
//        }

//        private static long ParseSnowflakeId(string id)
//        {
//            return long.TryParse(id, out long result) ? result : 0;
//        }
//    }
//}
