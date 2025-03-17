namespace iChat.BackEnd.Services.Users.Infra.IdGenerator
{
    public class ChannelIdService : SnowflakeService
    {
        public ChannelIdService(int workerId, int datacenterId) : base(workerId, datacenterId, 1742149073000L)
        {
        }
    }
}
