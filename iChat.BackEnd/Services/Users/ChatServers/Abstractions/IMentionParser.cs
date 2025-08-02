using iChat.DTOs.Collections;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IMentionParser
    {

        List<stringlong> ParseMentions(ChatMessageDto message);


       //string ParseMentionsWithDisplayNames(string message);
    }
}
