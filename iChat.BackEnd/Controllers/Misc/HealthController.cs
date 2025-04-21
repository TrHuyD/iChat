using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using iChat.BackEnd.Services.Users.Infra.Redis;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace iChat.BackEnd.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IDriver _neo4jDriver;
        private readonly Lazy<CasandraService> _casandraService;
        private readonly AppRedisService _redisService;

        public HealthController(IDriver neo4jDriver,Lazy<CasandraService> casandraService,AppRedisService appRedisService)
        {
            _redisService = appRedisService;
            _casandraService = casandraService;
            _neo4jDriver = neo4jDriver;
        }
        [HttpGet("Cassandra")]
        public async Task<IActionResult> CheckCassandra()
        {
            try
            {

                if (await _casandraService.Value.Health())
                    return Ok(new { status = "Healthy", message = "Connected to Neo4j successfully" });
                return StatusCode(500, new { status = "Unhealthy" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Unhealthy", error = ex.Message });
            }
        }

        [HttpGet("neo4j")]
        public async Task<IActionResult> CheckNeo4j()
        {
            try
            {
                await using var session = _neo4jDriver.AsyncSession();
                var result = await session.ExecuteReadAsync(tx =>
                    tx.RunAsync("RETURN 'Neo4j is up' AS message"));

                return Ok(new { status = "Healthy", message = "Connected to Neo4j successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Unhealthy", error = ex.Message });
            }
        }
        [HttpGet("redis")]
        public async Task<IActionResult> CheckRedis()
        {
            try
            {
                // Attempt to get a value from Redis to ensure the connection is healthy.
                var value = await _redisService.GetCacheValueAsync("health_check_key");

                // If the value is null, we could set a test value.
                if (value == null)
                {
                    await _redisService.SetCacheValueAsync("health_check_key", "OK", TimeSpan.FromSeconds(5));
                    value = await _redisService.GetCacheValueAsync("health_check_key");
                }

                return Ok(new { status = "Healthy", message = "Connected to Redis successfully", value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Unhealthy", error = ex.Message });
            }
        }

    }
}
