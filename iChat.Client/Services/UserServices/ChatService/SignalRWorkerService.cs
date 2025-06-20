// SignalRWorkerService.cs
using iChat.DTOs.Users.Messages;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iChat.Client.Services.UserServices.ChatService
{
    public class SignalRWorkerService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<SignalRWorkerService> _dotNetRef;
        private IJSObjectReference _worker;
        private bool _initialized = false;

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action OnReconnecting;
        public event Action OnReconnected;
        public event Action<List<ChatMessageDtoSafe>> OnMessageHistoryReceived;

        public SignalRWorkerService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _worker = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "signalRInterop.initialize", _dotNetRef);

            _initialized = true;
        }

        public async Task SendMessageAsync(string roomId, ChatMessageDtoSafe message)
        {
            if (!_initialized) throw new InvalidOperationException("Worker not initialized");
            await _worker.InvokeVoidAsync("sendMessage", roomId, message);
        }

        public async Task<List<ChatMessageDtoSafe>> GetMessageHistoryAsync(string roomId, string? beforeMessageId = null)
        {
            if (!_initialized) throw new InvalidOperationException("Worker not initialized");
            return await _worker.InvokeAsync<List<ChatMessageDtoSafe>>(
                "getMessageHistory", roomId, beforeMessageId);
        }

        [JSInvokable]
        public void HandleMessageReceived(string messageJson)
        {
            OnMessageReceived?.Invoke(messageJson);
        }

        [JSInvokable]
        public void HandleConnected()
        {
            OnConnected?.Invoke();
        }

        [JSInvokable]
        public void HandleDisconnected()
        {
            OnDisconnected?.Invoke();
        }

        [JSInvokable]
        public void HandleReconnecting()
        {
            OnReconnecting?.Invoke();
        }

        [JSInvokable]
        public void HandleReconnected()
        {
            OnReconnected?.Invoke();
        }

        [JSInvokable]
        public void HandleMessageHistory(List<ChatMessageDtoSafe> messages)
        {
            OnMessageHistoryReceived?.Invoke(messages);
        }

        [JSInvokable]
        public void HandleMessageHistoryError(string error)
        {
            Console.Error.WriteLine($"Error getting message history: {error}");
            OnMessageHistoryReceived?.Invoke(new List<ChatMessageDtoSafe>());
        }

        public async ValueTask DisposeAsync()
        {
            if (_worker != null)
            {
                try
                {
                    await _worker.InvokeVoidAsync("dispose");
                }
                finally
                {
                    await _worker.DisposeAsync();
                }
            }
            _dotNetRef?.Dispose();
        }
    }
}