using iChat.Client.Data;
using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        private async Task ProcessSendQueueAsync()
        {
            await foreach (var message in _sendQueue.Reader.ReadAllAsync())
            {
                try
                {
                    await ChatService.SendMessageAsync(ServerId, message);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to send message: {ex.Message}");
                }
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(_newMessage)) return;

            var messageContent = _newMessage.Trim();
            _newMessage = string.Empty;
            StateHasChanged();

            var message = new ChatMessageDtoSafe
            {
                Content = messageContent,
                MessageType = 1,
                ChannelId = RoomId,
                SenderId = _currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _sendQueue.Writer.WriteAsync(message);
            await Task.Delay(200);
            _shouldScrollToBottom = true;
        }
        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await SendMessage();
            }
        }
        private async Task AddMessages(BucketDto bucket)
        {
            var previousScroll = await JS.InvokeAsync<ScrollSnapshot>("captureScrollAnchor", _messagesContainer);

            foreach (var message in bucket.ChatMessageDtos)
            {
                _messages.TryAdd(long.Parse(message.Id), MessageRenderer.RenderMessage(message, _currentUserId));
            }
            _groupedMessages = await GroupMessagesAsync(_messages);
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("restoreScrollAfterPrepend", _messagesContainer, previousScroll);
        }

        private async Task<List<MessageGroup>> GroupMessagesAsync(SortedList<long, RenderedMessage> messages)
        {
            var groups = new List<MessageGroup>();
            MessageGroup? current = null;

            foreach (var msg in messages.Values.OrderBy(m => long.Parse(m.Message.Id)))
            {
                UserMetadataReact user = await _userMetadataService.GetUserByIdAsync(msg.Message.SenderId);
                if (current == null ||
                    current.UserId != msg.Message.SenderId ||
                    current.Messages.Count >= 5)
                {
                    current = new MessageGroup
                    {
                        UserId = msg.Message.SenderId,
                        User = user,
                        Timestamp = msg.Message.CreatedAt,
                        Messages = new List<RenderedMessage>()
                    };
                    groups.Add(current);
                }

                current.Messages.Add(msg);
            }
            return groups;
        }

        private async Task TryAddNewMessageToGroupAsync(RenderedMessage message)
        {
            var userId = message.Message.SenderId;
            var user = await _userMetadataService.GetUserByIdAsync(userId);
            var group = _groupedMessages.LastOrDefault(g => g.UserId == userId);
            if (group != null && group.CanAppend(message.Message))
            {
                group.Messages.Add(message);
            }
            else
            {
                _groupedMessages.Add(new MessageGroup
                {
                    UserId = userId,
                    User = user,
                    Messages = new List<RenderedMessage> { message }
                });
            }
        }





    }
}
