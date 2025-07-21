﻿using iChat.Client.Data;
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

        private Virtualize<MessageGroup>? virtualizeRef;


        private async Task AddMessagesBehind(MessageBucket bucket)
        {
            try
            {
                _lastScrollSnapshot = await JS.InvokeAsync<ScrollSnapshot>("captureScrollAnchor", _messagesContainer);
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
                    for (int i = newGroups.Count - 1; i >= 0; i--)
                    {
                        _groupedMessages.Insert(0, newGroups[i]);
                    }
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
            finally
            {
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
                    Select(_results[_highlight].UserId);

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
