namespace iChat.BackEnd.Models.Helpers
{
    public class RedisLiveTime
    {
        /// <summary>
        /// Chat Member
        /// </summary>
        public readonly TimeSpan sm;
        /// <summary>
        /// default channel
        /// </summary>
        public readonly TimeSpan dc;
        public RedisLiveTime(IConfiguration configuration)
        {
            var redisConfig = configuration.GetSection("RedisLiveTime");
            sm = TimeSpan.Parse(redisConfig["sm"] ?? throw new Exception("Chat member live time missing"));
            dc = TimeSpan.Parse(redisConfig["dc"] ?? throw new Exception("Root channel live time missing"));
        }
    }
}
