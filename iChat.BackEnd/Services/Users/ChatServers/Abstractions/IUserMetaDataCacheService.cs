using iChat.DTOs.Users;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IUserMetaDataCacheService
    {
        public  Task<UserMetadata?> GetAsync(string userId);
        public Task<(Dictionary<string, UserMetadata> dic,List<string> missing)> GetManyAsync(List<string> userIds);
        public Task SetAsync(UserMetadata user);
        public Task SetManyAsync(IEnumerable<UserMetadata> users);
        public Task SetPlaceholderAsync(string userId);
        public Task SetPlaceholdersAsync(IEnumerable<string> userIds);
        

    }
}
