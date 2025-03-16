using iChat.Data.Entities.Users;
using iChat.ViewModels.Users;
using Microsoft.AspNetCore.Identity;

namespace iChat.BackEnd.Services.Users
{
    public class PublicUserService :IPublicUserService
    {
        private readonly UserManager<AppUser> _userManager;
        public PublicUserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<UserVM> GetUserBasicInfo(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return null;
            return new UserVM() { Id=user.Id, Username=user.UserName};
        }
    }
}
