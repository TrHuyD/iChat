namespace iChat.BackEnd.Models.MessageSearch
{
    public class SearchOptions
    {
        public string QueryText { get; set; } = "";
        public long? ChannelId { get; set; }
        public long? ServerId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public string? SenderId { get; set; } = "";
        //public bool IncludeDeleted { get; set; } = false;
        public string SortBy { get; set; } = "timestamp";
        public bool SortDescending { get; set; } = true;
        public string? CursorToken { get; set; } = "";
    }
    public enum SearchScope
    {
        Channel,
        Server
    }
}
