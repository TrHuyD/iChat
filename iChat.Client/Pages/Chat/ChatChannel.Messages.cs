using iChat.Client.Data;
using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

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
        private InputFile _fileInputRef;
        private async Task TriggerFileInput()
        {
            await JS.InvokeVoidAsync("triggerInputFileClick", _fileInputRef.Element);
        }
        private async Task HandleFileUpload(InputFileChangeEventArgs e)
        {
                try
            {

                var inputFile = e.File;
                if (inputFile != null)
                    {
                        if (inputFile.Size > 2 * 1024 * 1024)
                        {
                            ToastService.ShowError("File is too large. (2MB limit)");
                            return;
                        }
                        using var stream = inputFile.OpenReadStream(5 * 1024 * 1024/2);
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();

                    var fileContent = new StreamContent(new MemoryStream(fileBytes));
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(inputFile.ContentType); 
                    var form = new MultipartFormDataContent();
                    form.Add(fileContent, "file", inputFile.Name);
                    form.Add(new StringContent(_currentChannel.Id.ToString()), "channelId");
                    form.Add(new StringContent(_currentServer.Id.ToString()), "serverId");

                    var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat/UploadMessage") { Content = form };
                    var response = await _https.SendAuthAsync(request);

                    if (response.IsSuccessStatusCode)
                        {
                        }
                        else
                        {
                        ToastService.ShowError($"Image upload failed.{response.StatusCode}");

                        }
                    }
            }
            catch(Exception ex)
            {
                ToastService.ShowError($"Image upload failed.{ex.Message}");

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
                ChannelId = ChannelId,
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
            else
            {

            }
        }
        private async Task AddMessagesBehind(MessageBucket bucket)
        {
            var previousScroll = await JS.InvokeAsync<ScrollSnapshot>("captureScrollAnchor", _messagesContainer);

            foreach (var message in bucket.ChatMessageDtos)
            {
                _messages.TryAdd(message.Id, MessageRenderer.RenderMessage(message));
            }
            if (_groupedMessages.Count == 0)
            {
                _groupedMessages = await GroupMessagesAsync(_messages);
            }
            else
            {
                var newGroups = await GroupMessagesAsync(bucket);
                foreach (var group in newGroups.Reverse<MessageGroup>()) // Keep order correct
                {
                    _groupedMessages.Insert(0, group);
                }
            }
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("restoreScrollAfterPrepend", _messagesContainer, previousScroll);
        }
        private async Task AddMessagesForward(MessageBucket bucket)
        {
            var previousScroll = await JS.InvokeAsync<ScrollSnapshot>("captureScrollAnchor", _messagesContainer);

            foreach (var message in bucket.ChatMessageDtos)
            {
                _messages.TryAdd(message.Id, MessageRenderer.RenderMessage(message));
            }
            if (_groupedMessages.Count == 0)
            {
                _groupedMessages = await GroupMessagesAsync(_messages);
            }
            else
            {
                var newGroups = await GroupMessagesAsync(bucket);
                foreach(var group in newGroups)
                _groupedMessages.Add( group);
            }
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("restoreScrollAfterPrepend", _messagesContainer, previousScroll);
        }

        private async Task<List<MessageGroup>> GroupMessagesAsync(MessageBucket bucket)
        {
            SortedList<long, RenderedMessage> messages= new SortedList<long, RenderedMessage>();
            foreach (var message in bucket.ChatMessageDtos)
            {
                messages.TryAdd(message.Id, MessageRenderer.RenderMessage(message));
            }
            return await GroupMessagesAsync(messages);
        }
        private async Task<List<MessageGroup>> GroupMessagesAsync(SortedList<long, RenderedMessage> messages)
        {
            var groups = new List<MessageGroup>();
            MessageGroup? current = null;

            foreach (var msg in messages.Values.OrderBy(m => m.Message.Id))
            {
                UserMetadataReact user =  _userMetadataService.GetUserByIdAsync(msg.Message.SenderId);
                if (current == null ||
                    current.UserId != msg.Message.SenderId ||
                    !current.CanAppend(msg.Message))
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
            var user =  _userMetadataService.GetUserByIdAsync(userId);
            if (_groupedMessages.Count != 0)
            {
                var group = _groupedMessages[^1];
                if (
                    group.CanAppend(message.Message))
                {
                    group.Messages.Add(message);
                    return;
                }
            }
            _groupedMessages.Add(new MessageGroup
            {
                UserId = userId,
                User = user,
                Messages = new List<RenderedMessage> { message },
                Timestamp = message.Message.CreatedAt
            });
        }





    }
}
