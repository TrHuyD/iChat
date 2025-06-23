using Blazored.LocalStorage;

namespace iChat.Client.Services.UserServices.Chat.Util
{
    public class LastVisitedChannelService
    {
        private readonly ILocalStorageService _localStorage;
        private const string StorageKeyPrefix = "last_channel_";

        public LastVisitedChannelService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SaveLastChannelAsync(string serverId, string channelId)
        {
            await _localStorage.SetItemAsync(StorageKeyPrefix + serverId, channelId);
        }

        public async Task<string?> GetLastChannelAsync(string serverId)
        {
            return await _localStorage.GetItemAsync<string>(StorageKeyPrefix + serverId);
        }
    }
}