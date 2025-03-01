using iChat.App.Models.Helper;
using Microsoft.AspNetCore.Mvc;

namespace iChat.App.Controllers.User
{
    [Route("api/[controller]")]
    public class UserController :Controller
    {
        private readonly HttpClient _httpClient;
        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientType.AuthApiClient);
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _httpClient.GetAsync("https://localhost:7296/api/User");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }
            //var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            return Ok();
        }
    }
}
