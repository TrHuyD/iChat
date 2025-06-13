using iChat.Client.Services.Auth;
using iChat.Client.Services.UserServices;
using iChat.Client.Services.UserServices.ChatService;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using System.Net;
using System.Net.Http.Json;

public class UserStateService
{
    private readonly JwtAuthHandler _authHandler;
    private readonly ChatNavigationService _chatNavigationService;


    private UserProfileDto? _userProfile;
    public static bool HasLoadedUserData { get; private set; } = false;
    public event Action OnAppReadyStateChanged;
    
    public UserStateService(JwtAuthHandler authHandler,
        ChatNavigationService chatNavigationService)
    {
        _authHandler = authHandler;
        _chatNavigationService = chatNavigationService;
    }
    public static void Reload()
    {
        HasLoadedUserData = false;
    }
    private async Task<UserCompleteDto> Load()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/CompleteInfo");
        var response = await _authHandler.SendAuthAsync(request);
        if(response.IsSuccessStatusCode)
        {
            var package = await response.Content.ReadFromJsonAsync<UserCompleteDto>();
            HasLoadedUserData = true;
            return package;
        }
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized|| response.StatusCode==System.Net.HttpStatusCode.BadRequest)
        {
            throw new HttpRequestException(message: "is not Log in", inner: null, HttpStatusCode.Unauthorized); }
        throw new HttpRequestException(message: "Server no responding", inner: null, statusCode: System.Net.HttpStatusCode.InternalServerError);
    }
    public async Task<bool> LoadAllDataAsync()
    {
        var package = await Load();
        _chatNavigationService.UpdateChatServers(package.ChatServers);
        _userProfile = package.UserProfile;
        return true;
    }
    //public async Task LoadServerList(List<ChatServerDto> list)
    //{
    //    var chatNavService = _serviceProvider.GetRequiredService<ChatNavigationService>();

    //}
    public long GetUserId()
    {
        if (_userProfile == null)
            throw new InvalidOperationException("User profile is not loaded yet.");
        return _userProfile.Id;
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
