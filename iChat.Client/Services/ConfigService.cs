namespace iChat.Client.Services
{
    public class ConfigService
    {
#if DEBUG
        public string ApiBaseUrl { get; set; } = "https://localhost:6051";
        public string baseurl { get; set; } = "https://localhost:7156";
#else
        public string ApiBaseUrl { get; set; } = "https://ichat.dedyn.io";
        public string baseurl { get; set; } = "https://ichat.dedyn.io";
#endif
    }

}
