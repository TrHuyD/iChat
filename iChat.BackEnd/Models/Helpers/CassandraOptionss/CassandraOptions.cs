namespace iChat.BackEnd.Models.Helpers.CassandraOptionss
{
    public class CassandraOptions
    {
        public string clientId {get;set;}
        public string secret { get; set; }
        public string token { get; set; }
        public string path { get; set; }
        public CassandraOptions(IConfiguration? config)
        {
            if (config is null)
                throw new Exception("Unable to load Cassandra options");
            clientId = config["clientId"]!;
            secret = config["secret"]!;
            token = config["token"]!;
            path = config["path"];

        }
    }
}
