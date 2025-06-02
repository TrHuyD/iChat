using iChat.DTOs.Users.Messages;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace iChat.Client.Services.UserServices.ChatService
{
    public class SignalRWorkerService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<SignalRWorkerService> _dotNetRef;
        private IJSObjectReference _worker;
        private bool _initialized = false;

        public event Action<string,string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action OnReconnecting;
        public event Action OnReconnected;

        public SignalRWorkerService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _worker = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "eval", "window.signalRInterop");

            // Initialize the worker with .NET reference and token
            var workerInstance = await _worker.InvokeAsync<IJSObjectReference>(
                "initialize", _dotNetRef);

            // Store the worker instance for later use
            _worker = workerInstance;
            _initialized = true;
        }

        public async Task JoinRoomAsync(string roomId)
        {
            if (!_initialized) throw new InvalidOperationException("Worker not initialized");
            await _worker.InvokeVoidAsync("joinRoom", roomId);
        }

        public async Task LeaveRoomAsync(string roomId)
        {
            if (!_initialized) return;
            await _worker.InvokeVoidAsync("leaveRoom", roomId);
        }

        public async Task SendMessageAsync(string roomId, ChatMessageDto message)
        {
            if (!_initialized) throw new InvalidOperationException("Worker not initialized");
            await _worker.InvokeVoidAsync("sendMessage", roomId, message);
        }

        [JSInvokable]
        public void HandleMessageReceived(string messageJson,string isMain)
        {
            OnMessageReceived?.Invoke(messageJson,isMain);
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