using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Components;

using System.Net.Http.Json;



namespace iChat.Client.Services.Auth
{
    public class JwtAuthHandler : DelegatingHandler
    {
        private readonly TokenProvider _tokenProvider;
        private readonly NavigationManager _navigation;
        private readonly ConfigService _configService;

        public JwtAuthHandler(TokenProvider tokenProvider, NavigationManager navigation,ConfigService configService)
        {
            _tokenProvider = tokenProvider;
            _navigation = navigation;
            _configService = configService;
        } 
 

        public async Task<HttpResponseMessage> SendAuthAsync(
            HttpRequestMessage request,
            bool forceRedirectToLogin = true,
            bool browser_cache=true,
            CancellationToken cancellationToken = default)
        {
            request.RequestUri = new Uri(_configService.ApiBaseUrl + request.RequestUri);

            if (!browser_cache)
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
            var token = await _tokenProvider.GetToken();
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

            //// Token missing or unauthorized
            //var reset_token = await _tokenProvider.RetrieveNewToken();
            //if (reset_token)
            //{

            //    request.Headers.Authorization =
            //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);

            //    return await base.SendAsync(request, cancellationToken);
            //}

            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                RequestMessage = request,
                Content = new StringContent("{\"error\": \"Token refresh failed\"}")
            };
        }

    }
}
