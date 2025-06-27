using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using System.Collections.Concurrent;

namespace iChat.Client.Services.UserServices.Chat
{
    public class ChatMessageService
    {
        //Channelid -> BucketId ->List<BucketDto> -> List<ChatMessageDtoSafe>>
        ConcurrentDictionary<long, SortedList<int, List<BucketDto>>> _messageCache = new();
        JwtAuthHandler _http;
        public ChatMessageService(JwtAuthHandler http)
        {
            _http = http;
        }
        private async Task Getbucket()
        {
            ///requesting missing buckets from the server
        }
        public async Task<List<BucketDto>> GetLatestMessage()
        {
            throw new NotImplementedException("GetMessage method is not implemented yet.");
        }
    }
}
