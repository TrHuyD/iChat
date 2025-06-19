using iChat.BackEnd.Services.Users.Auth;
using iChat.Data.Entities.Users;
using iChat.DTOs.Users;
using iChat.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
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

    public Task<UserCompleteDto?> GetUserCompleteInfoAsync(long userId)
    {
        throw new NotImplementedException();
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return null;

        return new UserProfileDto
        {
            Name = user.Name,
            Id = user.Id,
            AvatarUrl = $"https://cdn.discordapp.com/embed/avatars/index.png"
        };
    }
}
