using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace iChat.Client.Services.Auth
{
    public class SignalRConnectionFactory
    {
        private readonly TokenProvider _tokenProvider;
        private readonly NavigationManager _navigation;
        public SignalRConnectionFactory(TokenProvider tokenProvider, NavigationManager navigation)
        {
            _tokenProvider = tokenProvider;
            _navigation = navigation;
        }
        public HubConnection CreateHubConnection(string hubPath)
        {
            var token = _tokenProvider.AccessToken;
            return new HubConnectionBuilder()
                .WithUrl(_navigation.ToAbsoluteUri(hubPath), options =>
                {
                    options.AccessTokenProvider = async () => await _tokenProvider.GetToken();
                })
                .WithAutomaticReconnect()
                .Build();
        }
    }
}
