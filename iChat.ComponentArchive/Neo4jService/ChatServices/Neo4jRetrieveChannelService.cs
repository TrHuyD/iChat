using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.Infra.Redis;
using Neo4j.Driver;
using StackExchange.Redis;

namespace iChat.BackEnd.Services.Users.Infra.Neo4jService.ChatServices
{
    public class Neo4jRetrieveChannelService
    {
        private readonly IDriver _driver;

        public Neo4jRetrieveChannelService(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<string?> GetDefaultChannelIdAsync(string serverId)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.RunAsync(@"
            MATCH (s:ChatServer {id: $serverId})-[:DEFAULT_CHANNEL]->(c:Channel)
            RETURN c.id AS dfId",
                new { serverId });

            if (await result.FetchAsync())
            {
                return result.Current["dfId"].As<string>();
            }
            return null;
        }
    }

}
