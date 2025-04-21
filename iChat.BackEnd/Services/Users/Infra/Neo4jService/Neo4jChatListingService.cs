using Neo4j.Driver;

namespace iChat.BackEnd.Services.Users.Infra.Neo4jService
{
    public class Neo4jChatListingService
    {
        private readonly Lazy<IAsyncSession> __session;
        public Neo4jChatListingService(Lazy<IAsyncSession> session)
        {
            __session = session;
        }
        public async Task<List<string>> GetServerChannelListAsync(string serverId)
        {
            var query = @"
                    MATCH (s:ChatServer {id: toInteger($serverId)})-[:HAS_CHANNEL]->(c:ChatChannel)
                    RETURN collect(toString(c.id)) AS channelIds;
                    ";
            var _session = __session.Value;
            var result = await _session.RunAsync(query, new { serverId });
            var record = await result.SingleAsync();
            return record["channelIds"].As<List<string>>();
        }
        public async Task<List<string>> GetUserServersAsync(string userId)
        {
            var query = @"
                    MATCH (u:User {id: toInteger($userId)})-[:MEMBER_OF]->(s:ChatServer)
                    RETURN collect(toString(s.id)) AS serverIds;
                    ";
            var _session = __session.Value;
            var result = await _session.RunAsync(query, new { userId });
            var record = await result.SingleAsync();
            return record["serverIds"].As<List<string>>();

        }
        public async Task<List<string>> GetServerMembersAsync(string serverId)
        {
            var query = @"
                    MATCH (s:ChatServer {id: toInteger($serverId)})<-[:MEMBER_OF]-(u:User)
                    RETURN collect(toString(u.id)) AS userIds;
                    ";
            var _session = __session.Value;
            var result = await _session.RunAsync(query, new { serverId });
            var record = await result.SingleAsync();
            return record["userIds"].As<List<string>>();
        }
        //public async Task<bool>
    }
}
