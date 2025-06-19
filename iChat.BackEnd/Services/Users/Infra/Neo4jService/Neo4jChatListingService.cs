﻿using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
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
        public async Task<List<long>> GetServerChannelListAsync(string serverId)
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
        public async Task<List<long>> GetUserServersAsync(string userId)
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
        public async Task<List<long>> GetServerMembersAsync(string serverId)
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
        //public async Task<bool>
    }
}
