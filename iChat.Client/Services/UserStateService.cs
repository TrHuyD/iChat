using iChat.Client.Services.Auth;
using System.Net.Http.Json;

public class UserStateService
{
    private readonly JwtAuthHandler _authHandler;

    public UserStateService(JwtAuthHandler authHandler)
    {
        _authHandler = authHandler;
    }

    public async Task<UserProfileDto?> GetUserAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/users/profile");

        var response = await _authHandler.SendAuthAsync(request);

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
