using iChat.ViewModels.Users;

namespace iChat.BackEnd.Services.Users
{
    public interface IPublicUserService
    {
        public Task<UserVM> GetUserBasicInfo(string username);
    }
}
