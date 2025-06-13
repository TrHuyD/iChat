using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using Neo4j.Driver;

namespace iChat.BackEnd.Services.Users.Infra.Neo4jService
{
    public class CreateChatService
    {
        private readonly ChannelIdService _channelIdGen;
        private readonly ServerIdService _serverIdGen;
        private readonly IAsyncSession _session;
        public CreateChatService(ChannelIdService ChannelId,ServerIdService serverIdService,IAsyncSession session)
        {
            _channelIdGen = ChannelId;
            _serverIdGen = serverIdService;
            _session = session;
        }
        public async Task<long> CreateChannelAsync(long serverId, string channelName, long adminUserId)
        {
            var channelId = _channelIdGen.GenerateId().Id;
            var query = @"
                MATCH (s:ChatServer {id: $serverId})
                CREATE (c:Channel {id: $channelId, name: $channelName})
                CREATE (s)-[:HAS_CHANNEL]->(c)
                WITH c
                MATCH (u:User {id: $adminUserId})
                CREATE (u)-[:ADMIN_OF]->(c)";
            try
            {
                await _session.RunAsync(query, new { serverId, channelId, channelName, adminUserId });
                return channelId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating channel: {ex.Message}");
                return -1;
            }
        }
        public async Task<string> CreateServerAsync(string serverName, long adminUserId)
        {
            var serverId = _serverIdGen.GenerateId().Id;
            var channelId = _channelIdGen.GenerateId().Id;

            var query = @"
                MATCH (u:User {id: $adminUserId})
                CREATE (s:ChatServer {id: $serverId, name: $serverName})
                CREATE (c:ChatChannel {id: $channelId, name: 'general'})
                CREATE (s)-[:HAS_CHANNEL]->(c)
                CREATE (s)-[:DEFAULT_CHANNEL]->(c)  
                CREATE (u)-[:ADMIN_OF]->(s)
                CREATE (u)-[:MEMBER_OF]->(s)";

            try
            {
                await _session.RunAsync(query, new { serverId, channelId, serverName, adminUserId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating server: {ex.Message}");
            }
            return serverId.ToString();
        }

    }
}
