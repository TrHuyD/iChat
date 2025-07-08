namespace iChat.Client.Services.UserServices.Chat
{
    public class UserMetadataService
    {
        public async Task<UserMetadata> GetUserByIdAsync(string userId)
        {
            return await Task.FromResult(new UserMetadata(userId, "User", "default-avatar.png"));
        }
    }

    public record UserMetadata(string UserId, string DisplayName, string AvatarUrl);

}
