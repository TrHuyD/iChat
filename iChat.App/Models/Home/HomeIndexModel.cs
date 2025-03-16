using iChat.App.Models.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace iChat.App.Models.Home
{
    public class HomeIndexModel :PageModel
    {
        HttpClient _httpClient;
        public HomeIndexModel(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient.CreateClient(HttpClientType.AuthApiClient);
        }
        public async Task<IActionResult> OnGet()
        {
            var respone = await _httpClient.GetAsync("https://localhost:7296/api/User/profile");
            if(respone.IsSuccessStatusCode)
            {

            }
        }
    }
}
