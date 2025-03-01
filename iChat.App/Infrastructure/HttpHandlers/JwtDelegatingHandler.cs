using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using iChat.App.Models.Helper;

namespace iChat.App.Infrastructure.HttpHandlers
{
    public class JwtDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiAddressSettings _apiSettings;
        public JwtDelegatingHandler(IHttpContextAccessor httpContextAccessor,IOptions<ApiAddressSettings> options)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = options.Value;
        }
        private void AttachJwtToken(HttpRequestMessage request, string jwt)
        {
            if (!string.IsNullOrEmpty(jwt))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }
        }
        private void AttachJwtToken(HttpRequestMessage request, HttpContext httpContext)
        {
            AttachJwtToken(request,httpContext.Request.Cookies["jwt"]);
        }
        private async Task<string?> TryRefreshTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var refreshToken = httpContext.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return null;
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(_apiSettings.BaseUrl+ "/user/refreshtoken", new { refreshToken }, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var newJwt = await response.Content.ReadAsStringAsync();
                httpContext.Response.Cookies.Append("jwt", newJwt, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict });
                return newJwt;
            }
            return null;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return await base.SendAsync(request, cancellationToken);
            AttachJwtToken(request, context);
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
            
                var newJwt = await TryRefreshTokenAsync(context, cancellationToken);
                if (!string.IsNullOrEmpty(newJwt))
                {
                    AttachJwtToken(request, newJwt);
                    return await base.SendAsync(request, cancellationToken);
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
