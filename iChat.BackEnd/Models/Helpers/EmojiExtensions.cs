using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Servers;

namespace iChat.BackEnd.Models.Helpers
{
    public static class EmojiExtensions
    {
            public static EmojiBaseDto ToBaseDto(this Emoji emoji)
            {
            if (emoji == null) return null;
            return new EmojiBaseDto
            (emoji.Id, emoji.Name);
            }
        public static EmojiDetailDto ToDetailDto(this Emoji emoji)
        {
            if (emoji == null) return null;
            return new EmojiDetailDto
            (emoji.Id, emoji.Name,new DTOs.Collections.ServerId( emoji.ServerId));
        }


    }
}
