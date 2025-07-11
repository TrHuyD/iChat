

using iChat.Client.DTOs.DEV;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;


namespace iChat.Client.Services.Auth
{
    public class TokenProvider 
    {
        private string _accessToken = "";
        public string AccessToken
        {
            get
            {
                if (ExpireTime < DateTime.Now)
                    return "";
                return _accessToken;
            }
            private set
            {
                _accessToken = value;
            }
        }
        public DateTime ExpireTime { get; private set; } = DateTime.MinValue;
        public bool isAuthenticated => !string.IsNullOrEmpty(AccessToken);
        //public void SetToken(string token) => AccessToken = token;
        public void ClearToken()
        {
            AccessToken = "";
            ExpireTime = DateTime.MinValue;
        }
        
        private NavigationManager _navigation;
#if DEBUG

        private IJSRuntime _iJS;
        public TokenProvider(NavigationManager navigation,IJSRuntime jSRuntime)
        {
            _navigation = navigation;
            _iJS = jSRuntime;

        }

#else
        private HttpClient _httpClient;
        private ConfigService _configService;
public TokenProvider(NavigationManager navigation,HttpClient http,ConfigService config)
        {
            _configService = config;
            _navigation = navigation;
            _httpClient=http;
        }
#endif
		public async Task<RetrieveTokenResult> RetrieveNewToken()
        {
#if DEBUG
            var jsResponse = await _iJS.InvokeAsync<JsFetchResponse>("fetchwithcredentials", "https://localhost:6051/api/Auth/refreshtoken", null);
            var refreshResponse = jsResponse.ToHttpResponseMessage();

#else
            var refreshRequest = new HttpRequestMessage(HttpMethod.Get,_configService.ApiBaseUrl+ "/api/Auth/refreshtoken");
            var refreshResponse = await _httpClient.SendAsync(refreshRequest);
#endif
			if (refreshResponse.IsSuccessStatusCode)
            {
                var newToken = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();
                AccessToken = newToken!.AccessToken;
                ExpireTime =newToken!.ExpireTime;   
                return RetrieveTokenResult.Success;
            }
            if(refreshResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized||refreshResponse.StatusCode==System.Net.HttpStatusCode.BadRequest)
            {
                
                return RetrieveTokenResult.Unauthorized;
            }
            return RetrieveTokenResult.Fail;
        }
        private async Task ReadyToken()
        {
            if(string.IsNullOrWhiteSpace(AccessToken))
            {
                for (int i = 0; i < 3; i++)
                {
                    var rt = await RetrieveNewToken();
                    switch (rt)
                    {
                        case RetrieveTokenResult.Unauthorized:
                            _navigation.NavigateTo("/login");
                            throw new HttpRequestException("Fail to connect to Server", null, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        case RetrieveTokenResult.Success:
                            return;
                        default:
                            continue;
                    }

                    
                }
                throw new HttpRequestException("Fail to connect to Server", null, System.Net.HttpStatusCode.InternalServerError);
            }
            
        }
        public async Task<string> GetToken()
        {
            await ReadyToken();
            return AccessToken;
        }

        public enum RetrieveTokenResult
        {
            Success,
            Unauthorized,
            Fail
        }
    }
}
