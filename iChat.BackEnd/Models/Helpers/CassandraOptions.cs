namespace iChat.BackEnd.Models.Helpers
{
    public class CassandraOptions
    {
        public string ClientId {get;set;}
        public string Secret { get; set; }
        public string Token { get; set; }
        public string Path { get; set; }
        public CassandraOptions(IConfiguration? config)
        {
            if (config is null)
                throw new Exception("Unable to load Cassandra options");
            ClientId = config["clientId"]!;
            Secret = config["secret"]!;
            Token = config["token"]!;
            Path = config["path"];

        }
    }
}
