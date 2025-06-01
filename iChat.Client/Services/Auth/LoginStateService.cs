using Blazored.LocalStorage;

namespace iChat.Client.Services.Auth
{
    public class LoginStateService
    {
        private readonly ILocalStorageService _localStorage;
       
        public LoginStateService(ILocalStorageService LocalStorage)
        {
            _localStorage = LocalStorage;
        }
        private string cached;

        public async Task<bool> CheckedIfNotLogin()
        {
            if(cached==null)
                cached = await _localStorage.GetItemAsync<string>("LoginState");
            return cached == "false";
        }
        public async Task<bool> CheckedIfLogin()
        {
            if (cached == null)
                cached = await _localStorage.GetItemAsync<string>("LoginState");
            return cached == "true";
        }
        public async Task SetNotLogin()
        {
            cached = "false";
            await _localStorage.SetItemAsStringAsync("LoginState", "false");
        }
        public async Task SetLogin()
        {
            cached = "true";
            await _localStorage.SetItemAsStringAsync("LoginState", "true");
        }
    }
}
