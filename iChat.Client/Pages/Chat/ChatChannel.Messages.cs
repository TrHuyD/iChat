using iChat.Client.Data;
using iChat.Client.DTOs;
using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Collections;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

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
        private async Task EditMessage()
        {
            if (_editingMessageId == null || string.IsNullOrWhiteSpace(_newMessage))
                return;
            await _messageHandleService.EditMessageAsync( new UserEditMessageRq
            {
                ServerId=_currentServerId,
                ChannelId=_currentChannelId,
                MessageId=_editingMessageId.ToString(),
                NewContent = _newMessage.Trim()

            });

            ExitEditMode();
            _newMessage = "";
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



        private ScrollSnapshot? _lastScrollSnapshot;

        private Virtualize<RenderedMessage>? virtualizeRef;


private async Task AddMessagesBehind(MessageBucket bucket)
{
    try
    {
        _lastScrollSnapshot = await JS.InvokeAsync<ScrollSnapshot>("captureScrollAnchor", _messagesContainer);

        // Cache messages into main dictionary
        foreach (var message in bucket.ChatMessageDtos)
        {
            _messages.TryAdd(message.Id, MessageRenderer.RenderMessage(message));
        }

        List<RenderedMessage> newMessages;
        if (_renderedMessages.Count == 0)
        {
            newMessages = await GroupMessagesFlatAsync(bucket);
            _renderedMessages = newMessages;
        }
        else
        {
            newMessages = await GroupMessagesFlatAsync(bucket);
            _renderedMessages.InsertRange(0, newMessages);
        }

        StateHasChanged();

        if (virtualizeRef != null)
        {
            await virtualizeRef.RefreshDataAsync();
        }

        await JS.InvokeVoidAsync(
            "requestAnimationFrameThen",
            DotNetObjectReference.Create(this),
            nameof(RestoreScrollAfterPrepend)
        );
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error in AddMessagesBehind: {ex.Message}");
    }
}

        [JSInvokable]
        public async Task RestoreScrollAfterPrepend()
        {
            if (_lastScrollSnapshot is not null)
            {
                await JS.InvokeVoidAsync("restoreScrollAfterPrepend", _messagesContainer, _lastScrollSnapshot);
                _lastScrollSnapshot = null;
            }
        }
        private async Task HandleDeleteMessage(DeleteMessageRt rq)
        {
            if (rq.ChannelId != ChannelId) return;

            try
            {
                var messageId = long.Parse(rq.MessageId);

                if (_messages.TryGetValue(messageId, out var oldMessage))
                {
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling delete message: {ex.Message}");
            }
        }

        private async Task HandleEditMessage(EditMessageRt rq)
        {
            if (rq.ChannelId != ChannelId) return;

            try
            {
                var messageId = long.Parse(rq.MessageId);

                if (_messages.TryGetValue(messageId, out var oldMessage))
                {
                    oldMessage.Content = rq.NewContent;
                    await InvokeAsync(StateHasChanged);

                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling edit message: {ex.Message}");
            }
        }

        private async Task AddMessagesForward(MessageBucket bucket)
        {
            var previousScroll = await JS.InvokeAsync<ScrollSnapshot>("captureScrollAnchor", _messagesContainer);

            // Cache to dictionary
            foreach (var message in bucket.ChatMessageDtos)
            {
                _messages.TryAdd(message.Id, MessageRenderer.RenderMessage(message));
            }

            if (_renderedMessages.Count == 0)
            {
                _renderedMessages = await GroupMessagesFlatAsync(bucket);
            }
            else
            {
                var newMessages = await GroupMessagesFlatAsync(bucket);
                _renderedMessages.AddRange(newMessages);
            }

            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("restoreScrollAfterPrepend", _messagesContainer, previousScroll);
        }


        private List<RenderedMessage> _renderedMessages = new();
        private async Task<List<RenderedMessage>> GroupMessagesFlatAsync(MessageBucket bucket)
        {
            var ordered = bucket.ChatMessageDtos.OrderBy(m => m.CreatedAt);
            var result = new List<RenderedMessage>();

            long? lastUserId = null;
            DateTimeOffset? lastTimestamp = null;
            int groupCount = 0;

            foreach (var dto in ordered)
            {
                var rendered = MessageRenderer.RenderMessage(dto);
                var senderId = rendered.Message.SenderId;
                var timestamp = rendered.Message.CreatedAt;

                bool newGroup =
                    lastUserId != senderId ||
                    lastTimestamp == null ||
                    (timestamp - lastTimestamp.Value).TotalMinutes > 5 ||
                    groupCount >= 5;

                rendered.GroupCount = newGroup ? 0 : groupCount + 1;
                rendered.User = _userMetadataService.GetUserByIdAsync(senderId);

                result.Add(rendered);

                lastUserId = senderId;
                lastTimestamp = timestamp;
                groupCount = rendered.GroupCount;
            }

            return result;
        }

        private async Task HandleNewMessage(ChatMessageDto message)
        {
            if (message.ChannelId != RoomIdL) return;

            try
            {
                var rendered = MessageRenderer.RenderMessage(message);
                _messages.TryAdd(message.Id, rendered);

                await TryAddNewFlatMessageAsync(rendered);

                if (rendered.Message.SenderId.ToString() == _currentUserId)
                    _shouldScrollToBottom = true;

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling new message: {ex.Message}");
            }
        }
        private async Task TryAddNewFlatMessageAsync(RenderedMessage message)
        {
            var last = _renderedMessages.LastOrDefault();
            int groupCount = 0;

            if (last != null &&
                last.Message.SenderId == message.Message.SenderId &&
                (message.Message.CreatedAt - last.Message.CreatedAt).TotalMinutes <= 5 &&
                last.GroupCount < 5)
            {
                groupCount = last.GroupCount + 1;
            }

            message.GroupCount = groupCount;
            message.User = _userMetadataService.GetUserByIdAsync(message.Message.SenderId);
            _renderedMessages.Add(message);
        }






        private bool _isEditing = false;
        private long? _editingMessageId = null;
        private string _editingOriginalText = "";
        private string _editInputCache = "";
        private async Task BeginEditMessage()
        {
            _isEditing = true;
            _editingMessageId = _contextMenuMessage.Id;
            _editingOriginalText = _contextMenuMessage.Content;
            _editInputCache = _newMessage;
            _newMessage = _editingOriginalText;
            _showContextMenu = false;
            await Task.Yield(); 
            await inputEl.FocusAsync();
        }
        private void ExitEditMode()
        {
            _isEditing = false;
            _editingMessageId = null;
            _newMessage = _editInputCache;
        }
        private string _newMessage = string.Empty;
        private ElementReference inputEl;
        private bool _tracking = false;
        private int _mentionStart = -1;
        private bool _showDropdown = false;
        private List<UserMetadataReact> _results = new();
        private int _highlight = 0;
        private bool _preventDefault = false;
        private DateTime _next_time_Sending_Typing = DateTime.MinValue;
        double _dropdownTop = 0;
        double _dropdownLeft = 0;
        async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (_showDropdown && (e.Key == "ArrowDown" || e.Key == "ArrowUp" || e.Key == "Enter"))
            {
                _preventDefault = true;

                if (e.Key == "ArrowDown")
                    _highlight = (_highlight + 1) % _results.Count;
                else if (e.Key == "ArrowUp")
                    _highlight = (_highlight - 1 + _results.Count) % _results.Count;
                else if (e.Key == "Enter")
                    Select(_results[_highlight].userId.Value);

                return;
            }

            _preventDefault = false;

            if (e.Key == "@" && !_tracking)
            {
                int cursorPos = await JS.InvokeAsync<int>("mentionHelper.getCursorPos", inputEl);
                if (cursorPos == 0 || char.IsWhiteSpace(_newMessage[cursorPos - 1]))
                {
                    var pos = await JS.InvokeAsync<Coords>("mentionHelper.getCursorCoordinates", inputEl);
                    _dropdownTop = pos.Top;
                    _dropdownLeft = pos.Left;
                    _tracking = true;
                    _mentionStart = cursorPos;
                }
            }

            if (e.Key == "Escape" && _isEditing)
            {
                ExitEditMode();
                return;
            }

            if (e.Key == "Enter" && !e.ShiftKey)
            {
                if (_isEditing)
                    await EditMessage();
                else
                    await SendMessage();
            }
            else if (_next_time_Sending_Typing < DateTime.UtcNow)
            {
                _ = ChatService.Typing();
                _next_time_Sending_Typing = DateTime.UtcNow.AddSeconds(3);
            }
        }

        async Task HandleInput(ChangeEventArgs e)
        {
            _newMessage = e.Value?.ToString() ?? "";
            if (!_tracking) return;

            var fragment = GetFragment(_newMessage, _mentionStart);
            if (!string.IsNullOrWhiteSpace(fragment))
            {
                _results = _userMetadataService.SearchUsers(fragment);
                _showDropdown = _results.Any();
                _highlight = 0;
            }
            else
            {
                _showDropdown = false;
            }
        }


        string GetFragment(string text, int start)
        {
            if (start < 0 || start >= text.Length) return "";
            int i = start;
            while (i < text.Length && !char.IsWhiteSpace(text[i])) i++;
            return text.Substring(start + 1, i - start - 1);
        }

        void Select(stringlong user)
        {
            var frag = GetFragment(_newMessage, _mentionStart);
            int end = _mentionStart + frag.Length + 1;
            _newMessage = _newMessage[.._mentionStart] + "<" + user +">"+ _newMessage[end..];
            _tracking = _showDropdown = false;
            StateHasChanged();
        }

    }
}
