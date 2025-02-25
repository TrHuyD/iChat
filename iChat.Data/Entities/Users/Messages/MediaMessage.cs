using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users.Messages
{
    public class MediaMessage : Message
    {
        public string UrlCollection { get; private set; } = "[]";
        public List<string> Urls
        {
            get => JsonSerializer.Deserialize<List<string>>(UrlCollection) ?? [];
            set => UrlCollection = JsonSerializer.Serialize(value);
        }
    }
}
