using iChat.BackEnd.Services.Users.Auth;
using iChat.Data.Entities.Users;
using iChat.DTOs.Collections;
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

    public Task<UserCompleteDto?> GetUserCompleteInfoAsync(UserId userId)
    {
        throw new NotImplementedException();
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(UserId userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return null;

        return new UserProfileDto
        {
            Name = user.Name,
            Id = user.Id,
            AvatarUrl= user.AvatarUrl ?? $"https://cdn.discordapp.com/embed/avatars/0.png"
        };
    }
    public async Task<UserMetadata> GetUserMetadataAsync(UserId userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()   );
        if (user == null)
            throw new Exception("User not found");
        return new UserMetadata
         (
            userId,
            user.Name,
            user.AvatarUrl ?? $"https://cdn.discordapp.com/embed/avatars/0.png"
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
                new UserId(u.Id),
                u.Name,
               u.AvatarUrl?? $"https://cdn.discordapp.com/embed/avatars/0.png"
            ))
            .ToListAsync();

        return users;
    }

    public async Task<UserMetadata> EditUserNickNameAsync(UserId userId, string newNickName)
    {
        if(newNickName.Length > 50||string.IsNullOrWhiteSpace(newNickName))
            throw new ArgumentException("Nickname cannot exceed 50 characters.");
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if(user == null)
            throw new Exception("User not found");
        user.Name = newNickName;
        try
        {
            var result = await _userManager.UpdateAsync(user);
            
        }
        catch(Exception ex)
        {
            throw new Exception("Failed to update user nickname", ex);
        }
        return new UserMetadata
        (
             new UserId(user.Id),
            newNickName,
            user.AvatarUrl ?? $"https://cdn.discordapp.com/embed/avatars/0.png"
        );
    }
    public async Task<UserMetadata> EditAvatarAsync(UserId userId, string avatarUrl)
    {

        var user =await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new Exception("User not found");
        user.AvatarUrl = avatarUrl;
        try
        {
            var result = await _userManager.UpdateAsync(user);

        }
        catch (Exception ex)
        {
            throw new Exception("Failed to update user avatar url", ex);
        }
    return new UserMetadata
        (
            new UserId (user.Id),
            user.UserName,
            avatarUrl
        );
    }

    public async Task<UserMetadata> EditNameAndAvatarAsync(UserId userId, string newNickName, string avatarUrl)
    {

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new Exception("User not found");
        user.AvatarUrl = avatarUrl;
        user.Name=newNickName;
        try
        {
            var result = await _userManager.UpdateAsync(user);

        }
        catch (Exception ex)
        {
            throw new Exception("Failed to update user avatar url", ex);
        }
        return new UserMetadata
            (
                 new UserId(user.Id),
                user.Name,
                avatarUrl
            );
    }
}
