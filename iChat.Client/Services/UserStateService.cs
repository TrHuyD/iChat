﻿using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using System.Net.Http.Json;

public class UserStateService
{
    private readonly JwtAuthHandler _authHandler;
    private UserProfileDto? _userProfile;


    public UserStateService(JwtAuthHandler authHandler)
    {
        _authHandler = authHandler;
    }

    public async Task<UserProfileDto?> GetUserAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/profile");

        var response = await _authHandler.SendAuthAsync(request,browser_cache:false);

        if (!response.IsSuccessStatusCode)
        { 
            if (response.StatusCode ==System.Net.HttpStatusCode.Unauthorized)
                //       return null;
                return null;
            throw new HttpRequestException(message: "Server no responding", inner:null, statusCode: System.Net.HttpStatusCode.InternalServerError);
        }
        return await response.Content.ReadFromJsonAsync<UserProfileDto>();
    }
}
