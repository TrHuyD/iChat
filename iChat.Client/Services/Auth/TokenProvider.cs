namespace iChat.Client.Services.Auth
{
    public class TokenProvider 
    {
        public string? AccessToken { get; private set; }
        public bool isAuthenticated => !string.IsNullOrEmpty(AccessToken);
        public void SetToken(string token) => AccessToken = token;
        public void ClearToken() => AccessToken = null;
    }
}
