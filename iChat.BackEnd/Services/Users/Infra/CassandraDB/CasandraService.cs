using ISession= Cassandra.ISession;
using Cassandra;
using iChat.BackEnd.Models.Helpers;

namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class CasandraService
    {

        protected readonly ISession session;
        public CasandraService(CassandraOptions options)
        {
            session = Cluster.Builder()
           .WithCloudSecureConnectionBundle(options.Path)
           .WithCredentials(options.ClientId, options.Secret)
           .Build()
           .Connect();
        }
        public async Task<bool> Health()
        {
            try
            {
                var rs = await session.ExecuteAsync(new SimpleStatement("SELECT release_version FROM system.local"));
                return rs.Any();
            }
            catch
            {
                return false;
            }
        }
    }
}
