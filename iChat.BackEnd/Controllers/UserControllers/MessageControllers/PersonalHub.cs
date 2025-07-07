using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    [Authorize]
    public class PersonalHub : Hub
    {
        public string PrivateGroupName(string userId) => $"Private_{userId}";
        public async Task UpdateLastSeen_BE(LastSeenUpdateRequest request, [FromServices]IMessageLastSeenService seenService)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await seenService.UpdateLastSeenAsync(request.ChatChannelId, request.ServerId, request.MessageId, userId);
            await Clients.Group(PrivateGroupName(userId)).SendAsync("LastSeenUpdate_Client", new LastSeenUpdateResponse
            {
                ChatChannelId = request.ChatChannelId,
                ServerId = request.ServerId,
                MessageId = request.MessageId,
                timestamp = DateTimeOffset.UtcNow
            });
        }
    }
}
