using iChat.ViewModels.Users;
using Neo4j.Driver;

public class UserRelationService
{
    private readonly IAsyncSession _session;

    public UserRelationService(IAsyncSession session)
    {
        _session = session;
    }

    public async Task<List<string>> GetUserFriendsAsync(string userId)
    {
        return await _session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(@"
                MATCH (u:User)-[:FRIEND_WITH]->(friend:User) 
                WHERE u.id = $userId 
                RETURN friend.name AS FriendName",
                new { userId });

            return (await result.ToListAsync(r => r["FriendName"].As<string>()));
        });
    }

    public async Task CreateUserNode(long id)
    {
        await _session.ExecuteWriteAsync(tx =>
            tx.RunAsync("MERGE (u:User {id: $id})", new { id })
        );
    }
}
