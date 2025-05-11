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

        public async Task<HttpResponseMessage> SendAuthAsync(
            HttpRequestMessage request,
            bool forceRedirectToLogin = true,
            CancellationToken cancellationToken = default)
        {
            var token = _tokenProvider.AccessToken;
            request.RequestUri = new Uri(request.RequestUri.ToString());
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
            var refreshRequest = new HttpRequestMessage(HttpMethod.Get,  "/refreshToken");
            var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);

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
