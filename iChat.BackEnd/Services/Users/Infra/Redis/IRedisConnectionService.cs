using StackExchange.Redis;

namespace iChat.BackEnd.Services.Users.Infra.Redis
{
    public interface IRedisConnectionService
    {
        public IDatabase GetDataBase();
        public ConnectionMultiplexer connectionMultiplexer();
    }
}
