namespace iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices
{
    public partial class RedisChatServerService
    {
        string CreateInviteLink(string serverId, string userId)
        {
            return $"{serverId}:{userId}";
        }
    }
}
