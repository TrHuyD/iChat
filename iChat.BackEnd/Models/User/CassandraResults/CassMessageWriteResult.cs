namespace iChat.BackEnd.Models.User.CassandraResults
{
    public class CassMessageWriteResult
    {
        public bool Success { get; set; }
       // public long MessageId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
