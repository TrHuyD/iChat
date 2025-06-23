using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Neo4j.Driver;

namespace iChat.BackEnd.Services.Users.Infra.Neo4jService
{
    public class Neo4jChatListingService : IChatListingService
    {
        private readonly Lazy<IAsyncSession> __session;
        public Neo4jChatListingService(Lazy<IAsyncSession> session)
        {
            __session = session;
        }
        public async Task<List<long>> GetServerChannelListAsync(long serverId)
        {
            var query = @"
                    MATCH (s:ChatServer {id: toInteger($serverId)})-[:HAS_CHANNEL]->(c:ChatChannel)
                    RETURN collect(toString(c.id)) AS channelIds;
                    ";
            var _session = __session.Value;
            var result = await _session.RunAsync(query, new { serverId });
            var record = await result.SingleAsync();
            return record["channelIds"].As<List<long>>();
        }
        public async Task<List<long>> GetUserServersAsync(long userId)
        {
            var query = @"
                    MATCH (u:User {id: toInteger($userId)})-[:MEMBER_OF]->(s:ChatServer)
                    RETURN collect(toString(s.id)) AS serverIds;
                    ";
            var _session = __session.Value;
            var result = await _session.RunAsync(query, new { userId });
            var record = await result.SingleAsync();
            return record["serverIds"].As<List<long>>();

        }
        public async Task<List<long>> GetServerMembersAsync(long serverId)
        {
            var query = @"
                    MATCH (s:ChatServer {id: toInteger($serverId)})<-[:MEMBER_OF]-(u:User)
                    RETURN collect(toString(u.id)) AS userIds;
                    ";
            var _session = __session.Value;
            var result = await _session.RunAsync(query, new { serverId });
            var record = await result.SingleAsync();
            return record["userIds"].As<List<long>>();
        }

        Task<List<long>> IChatListingService.GetServerChannelListAsync(long serverId)
        {
            throw new NotImplementedException();
        }

        Task<List<long>> IChatListingService.GetUserServersAsync(long userId)
        {
            throw new NotImplementedException();
        }

        Task<List<long>> IChatListingService.GetServerMembersAsync(long serverId)
        {
            throw new NotImplementedException();
        }

        Task<List<ChatServerDto>> IChatListingService.GetUserChatServersAsync(long userId)
        {
            throw new NotImplementedException();
        }
        //public async Task<bool>
    }
}
