namespace iChat.BackEnd.Services.Users.Infra.IdGenerator
{
    public class ServerIdService : SnowflakeService
    {
        public ServerIdService(int workerId, int datacenterId) : base(workerId, datacenterId, 1647429473000L)
        {
        }
    }
}
