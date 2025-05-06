using iChat.BackEnd.Services.Users.Auth;
using iChat.Data.Entities.Users;
using iChat.ViewModels.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neo4j.Driver;
public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;

    public UserService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }


    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        return new UserProfileDto
        {
            Name = user.Name,
            //AvatarUrl = $"/profile?avatarid={user.Id}"
        };
    }
}
