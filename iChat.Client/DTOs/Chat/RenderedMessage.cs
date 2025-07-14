using iChat.DTOs.Users.Messages;

namespace iChat.Client.DTOs.Chat
{
    public class RenderedMessage
    {
        public ChatMessageDto Message { get; set; }
        public string CssClass { get; set; } = "";
        public string? Icon { get; set; }
        public string Content { get; set; } = "";
        public bool ShowTimestamp { get; set; }
        public bool isEdited { get; set; } = false;
        public static string DeleteMessage = "[This message has been deleted.]";
        public void ToggleDelete()
        {
            Message.Content = DeleteMessage;
            Message.IsDeleted = true;
        //    CssClass = "deleted-message";
            Content =DeleteMessage;
        }   
        public void HandleEdit(string newContent)
        {
            if(Message.IsDeleted)
            {
                return; // Cannot edit a deleted message
            }
            isEdited = true;
          //  CssClass = "edited-message";
            Content = newContent;
            Message.Content = newContent;
        }
    }

}
