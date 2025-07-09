namespace iChat.DTOs.Users.Messages
{
    public class PaginatedResult<T>
    {
        public IList<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string NextPageToken { get; set; }
        public string PreviousPageToken { get; set; }
    }
}
