using ISession = Cassandra.ISession;
using Cassandra;
using iChat.BackEnd.Models.Helpers.CassandraOptionss;
using Microsoft.Extensions.Logging;
using Polly;

namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class CasandraService : IDisposable
    {
        private readonly ISession _session;
        private readonly CassandraOptions _options;
        private Cluster _cluster;

        public CasandraService(CassandraOptions options)
        {
            _options = options;
            _session = ConnectToCassandra();
        }
        public ISession GetSession() => _session;
        private ISession ConnectToCassandra()
        {
            try
            {
                Console.WriteLine("Connecting to Cassandra...");

                _cluster = Cluster.Builder()
                    .WithCloudSecureConnectionBundle(_options.path)
                    .WithCredentials(_options.clientId, _options.secret)
                    .WithReconnectionPolicy(new ExponentialReconnectionPolicy(1000, 60000))
                    .WithPoolingOptions(new PoolingOptions()
                        .SetCoreConnectionsPerHost(HostDistance.Local, 1)   // Minimum active connections per host
                        .SetMaxConnectionsPerHost(HostDistance.Local, 8)   // Maximum connections per host
                      //  .SetCoreConnectionsPerHost(HostDistance.Remote, 2) // For remote nodes
                     //   .SetMaxConnectionsPerHost(HostDistance.Remote, 4)
                    //    .SetMaxRequestsPerConnection( 2048)
                    )
                    .Build();


                var session = _cluster.Connect();
                Console.WriteLine("Connected to Cassandra.");
                return session;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cassandra connection failed w {ex.Message}");
                throw;
            }
        }

        public async Task<bool> Health()
        {
            var retryPolicy = Policy
                .Handle<Cassandra.NoHostAvailableException>()
                .Or<Cassandra.OperationTimedOutException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Health check failed. Retry {retryCount}/3 in {timeSpan}. Error: {exception.Message}");
                    });

            return await retryPolicy.ExecuteAsync(async () =>
            {
                var rs = await _session.ExecuteAsync(new SimpleStatement("SELECT release_version FROM system.local"));
                return rs.Any();
            });
        }

        public void Dispose()
        {
            _session?.Cluster?.Dispose();
        }
    }
}
