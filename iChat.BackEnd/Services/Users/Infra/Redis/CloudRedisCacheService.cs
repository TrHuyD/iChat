using StackExchange.Redis;

namespace iChat.BackEnd.Services.Users.Infra.Redis
{
    public class CloudRedisCacheService :IRedisConnectionService
    {

        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        public CloudRedisCacheService(IConfiguration configuration) 
        {
            var redisConfig = configuration.GetSection("Redis");
            var host = redisConfig["Uri"] ?? throw new ArgumentNullException("Redis Host missing");
            var port = int.Parse(redisConfig["Port"] ?? "15253");
            var user = "default";
            var password = redisConfig["Password"];
            var options = new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                User = user,
                Password = password,
                ClientName = "RedisCacheService",
                ConnectTimeout = 5000,
                ConnectRetry = 3,
                AbortOnConnectFail = false,
                DefaultDatabase = 0,
                SocketManager = new SocketManager("RedisSockets")
            };
            _redis = ConnectionMultiplexer.Connect(options);
            _db = _redis.GetDatabase();
        }
        public IDatabase GetDataBase() => _db;
        public ConnectionMultiplexer connectionMultiplexer() => _redis;

    }
}
