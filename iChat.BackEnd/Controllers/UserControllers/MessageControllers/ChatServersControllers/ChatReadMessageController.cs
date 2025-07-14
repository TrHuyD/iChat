using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [Route("api/ChatChannel")]
    [ApiController]
    [Authorize]
    public class ChatReadMessageController : ControllerBase
    {
        public ChatReadMessageController()
        {
        }
   
        [HttpGet("{channelId:long}/buckets")]
        public async Task<IActionResult> GetBucketsInRange(
            [FromServices] IMessageDbReadService readService,
            long channelId,
            [FromQuery(Name = "endid")] int endId,
            [FromQuery(Name = "limit")] int limit)
        {
            if (channelId <= 1_000_000_000_000_000)
                return BadRequest("Invalid channel ID.");
            if (endId < 0 || limit <= 0 || limit > 3)
                return BadRequest("Invalid bucket ID range.");
            var buckets = await readService.GetBucketsInRangeAsync(channelId, endId - limit + 1, endId, 150);
            return Ok(buckets);
        }
        [HttpGet("{channelId}/latest")]
        public async Task<IActionResult> test([FromServices] IMessageReadService readService, string channelId)
        {
            if (long.TryParse(channelId, out long channelIdLong) == false || (channelIdLong <= 1000000000000000l))
            {
                return BadRequest("Invalid channel ID format.");
            }
            return Ok(await readService.GetLatestBucketsAsync(channelIdLong));
        }
        [HttpGet("{channelId:long}/bucketsingle")]
        public async Task<IActionResult> GetBucketById(
            [FromServices] IMessageReadService readService,
            long channelId,
            [FromQuery(Name = "id")] int bucketId)
        {
            if (channelId <= 1_000_000_000_000_000)
                return BadRequest("Invalid channel ID.");
            if (bucketId < 0)
                return BadRequest("Invalid bucket ID.");
            try
            {
                var bucket = await readService.GetBucketByIdAsync(channelId, bucketId);
                return Ok(bucket);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving bucket: {ex.Message}");
            }
            
        }
        
    }
}