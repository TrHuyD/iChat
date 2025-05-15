using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Components;

using System.Net.Http.Json;


#if DEBUG
using iChat.Client.DTOs.DEV;
using Microsoft.JSInterop;
#endif

namespace iChat.Client.Services.Auth
{
    public class JwtAuthHandler : DelegatingHandler
    {
        private readonly TokenProvider _tokenProvider;
        private readonly NavigationManager _navigation;


#if DEBUG
        private readonly IJSRuntime _iJS;
        public JwtAuthHandler(TokenProvider tokenProvider, NavigationManager navigation, IJSRuntime iJS)
        {
            _tokenProvider = tokenProvider;
            _navigation = navigation;
            _iJS = iJS;
        }
#else
        public JwtAuthHandler(TokenProvider tokenProvider, NavigationManager navigation)
        {
            _tokenProvider = tokenProvider;
            _navigation = navigation;
        }
#endif

        public async Task<HttpResponseMessage> SendAuthAsync(
            HttpRequestMessage request,
            bool forceRedirectToLogin = true,
            bool browser_cache=true,
            CancellationToken cancellationToken = default)
        {
#if DEBUG
            request.RequestUri = new Uri("https://localhost:6051" + request.RequestUri);
#endif
            var token = _tokenProvider.AccessToken;
            if(!browser_cache)
            {
                request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };
                request.Headers.Pragma.ParseAdd("no-cache");
                request.Headers.IfModifiedSince = DateTimeOffset.UtcNow;
            }
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return response;

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // retry once
                    response = await base.SendAsync(request, cancellationToken);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return response;


                    return response;
                }

                if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                    return response;
            }

            // Token missing or unauthorized
#if DEBUG
            var jsResponse = await _iJS.InvokeAsync<JsFetchResponse>("fetchWithCredentials", "https://localhost:6051/api/Auth/refreshtoken", null);
            var refreshResponse = jsResponse.ToHttpResponseMessage();

#else
            var refreshRequest = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/refreshtoken");
            var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);
#endif
            if (refreshResponse.IsSuccessStatusCode)
            {
                var newToken = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();
                _tokenProvider.SetToken(newToken!.AccessToken);

                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken.AccessToken);

                return await base.SendAsync(request, cancellationToken);
            }

            _tokenProvider.ClearToken();
            if (forceRedirectToLogin)
                _navigation.NavigateTo("/login", forceLoad: false);

            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                RequestMessage = request,
                Content = new StringContent("{\"error\": \"Token refresh failed\"}")
            };
        }

    }
}
