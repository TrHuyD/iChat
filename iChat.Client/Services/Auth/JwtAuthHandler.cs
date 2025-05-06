using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace iChat.Client.Services.Auth
{
    public class JwtAuthHandler : DelegatingHandler
    {
        private readonly TokenProvider _tokenProvider;

        private readonly NavigationManager _navigation;
        public JwtAuthHandler(TokenProvider tokenProvider, NavigationManager navigation)
        {
            _tokenProvider = tokenProvider;
            _navigation = navigation;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_tokenProvider.AccessToken))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Try refresh
                var refresh = new HttpRequestMessage(HttpMethod.Get, "/refreshToken");
                refresh.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
                var refreshResp = await base.SendAsync(refresh, cancellationToken);

                if (refreshResp.IsSuccessStatusCode)
                {
                    var newToken = await refreshResp.Content.ReadFromJsonAsync<TokenResponse>();
                    _tokenProvider.SetToken(newToken!.AccessToken);
                    // Retry original request
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken.AccessToken);
                    return await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    _tokenProvider.ClearToken();
                    _navigation.NavigateTo("/login");

                }
            }

            return response;
        }
    }

}
