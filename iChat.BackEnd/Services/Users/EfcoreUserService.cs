using iChat.BackEnd.Services.Users.Auth;
using iChat.Data.Entities.Users;
using iChat.DTOs.Users;
using iChat.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neo4j.Driver;
using System.Linq;

public class EfcoreUserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;

    public EfcoreUserService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public Task<UserCompleteDto?> GetUserCompleteInfoAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
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
    public async Task<UserMetadata> GetUserMetadataAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");
        return new UserMetadata
         (
            userId,
            user.Name,
            $"https://cdn.discordapp.com/embed/avatars/index.png"
        );
    }
    public async Task<List<UserMetadata>> GetUserMetadataBatchAsync(List<string> userIds)
    {
        if(userIds.Count>50)
            throw new ArgumentException("Batch size exceeds the limit of 50 users.");
        var useridslong= userIds.Select(id => long.Parse(id)).ToList();
        var users = await _userManager.Users
            .Where(u => useridslong.Contains(u.Id))
            .Select(u => new UserMetadata(
                u.Id.ToString(),
                u.Name,
                $"https://cdn.discordapp.com/embed/avatars/index.png"
            ))
            .ToListAsync();

        return users;
    }
}
