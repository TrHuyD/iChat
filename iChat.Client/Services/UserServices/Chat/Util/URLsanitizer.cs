﻿namespace iChat.Client.Services.UserServices.Chat.Util
{
    public static class URLsanitizer
    {
        public static string Apply(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                url = "https://cdn.discordapp.com/embed/avatars/0.png";
            }
            else
     if (!url.StartsWith("http"))
            {
#if DEBUG
                url = "https://localhost:6051" + url;
#else
                                        url = "https://ichat.dedyn.io" +url;
#endif
            }
            return url;
        }
    }
}
