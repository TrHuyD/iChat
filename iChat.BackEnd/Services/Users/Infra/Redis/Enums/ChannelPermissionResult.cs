namespace iChat.BackEnd.Services.Users.Infra.Redis.Enums
{
    public enum ChannelPermissionResult
    {
        NoAccess = 20,
        ReadAccess = 21,
        WriteAccess = 22,
        AdminAccess = 23,
        ChannelListExpired = 24,
        NotInServer = 10,
        MembershipExpired = 12,
        Unknown = -1
    }

}
