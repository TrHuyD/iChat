using Cassandra;
using Neo4j.Driver;
namespace iChat.BackEnd.Services.Users.Infra.Neo4jService
{
    public class ChatRoleService
    {
        private readonly IAsyncSession _session;

        public ChatRoleService(IAsyncSession session)
        {
            _session = session;
        }

        public async Task AssignRoleToUser(string userId, string serverId, string roleId)
        {
            var query = @"
            MATCH (u:User {id: $userId}), (s:Server {id: $serverId}), (r:Role {id: $roleId})
            MERGE (u)-[:HAS_ROLE]->(r)-[:BELONGS_TO]->(s)";

            await _session.RunAsync(query, new { userId, serverId, roleId });
        }

        public async Task SetChannelPermissions(string roleId, string channelId, string permission)
        {
            var query = @"
            MATCH (r:Role {id: $roleId}), (c:Channel {id: $channelId})
            MERGE (r)-[:CAN_ACCESS {permission: $permission}]->(c)";

            await _session.RunAsync(query, new { roleId, channelId, permission });
        }

        public async Task<bool> CanUserAccessChannel(string userId, string channelId, string requiredPermission)
        {
            var query = @"
            MATCH (u:User {id: $userId})-[:HAS_ROLE]->(r)-[rel:CAN_ACCESS]->(c:Channel {id: $channelId})
            WHERE rel.permission = $requiredPermission
            RETURN COUNT(r) > 0 AS hasAccess";

            var result = await _session.RunAsync(query, new { userId, channelId, requiredPermission });
            return await result.SingleAsync(r => r["hasAccess"].As<bool>());
        }
        public async Task<bool> RenameRoleAsync(string roleId, string newName)
        {
            var query = @"
        MATCH (r:Role {id: $roleId})
        SET r.name = $newName
        RETURN r";

            var result = await _session.RunAsync(query, new { roleId, newName });
            var record = await result.ToListAsync();

            return record.Count() != 0;
        }
        public async Task<bool> UpdateRolePermissionsAsync(string roleId, string channelId, string newPermission)
        {
            var query = @"
            MATCH (r:Role {id: $roleId})-[rel:CAN_ACCESS]->(c:Channel {id: $channelId})
            DELETE rel
            MERGE (r)-[:CAN_ACCESS {permission: $newPermission}]->(c)
            RETURN r";
            var result = await _session.RunAsync(query, new { roleId, channelId, newPermission });
            var record = await result.ToListAsync();
            return record.Count() != 0;

        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            var query = @"
            MATCH (r:Role {id: $roleId})
            DETACH DELETE r
            RETURN r";
            var result = await _session.RunAsync(query, new { roleId });
            var record = await result.ToListAsync();
            return record.Count() != 0;
        }
    }


}