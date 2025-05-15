
using System.Net;
using System.Text.Json;
using System.Text;

namespace iChat.Client.DTOs.DEV
{
#if DEBUG
    public class JsFetchResponse
    {
        public bool Ok { get; set; }
        public int Status { get; set; }
        public object? Json { get; set; }
        public HttpResponseMessage ToHttpResponseMessage()
        {
            var response = new HttpResponseMessage((HttpStatusCode)Status);

            if (Json != null)
            {
                var jsonString = JsonSerializer.Serialize(Json);
                response.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }

            return response;
        }
    }
#endif
}

