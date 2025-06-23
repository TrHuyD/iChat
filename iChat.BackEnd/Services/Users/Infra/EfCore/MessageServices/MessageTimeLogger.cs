namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class MessageTimeLogger
    {
        public MessageTimeLogger() { }
        private DateTimeOffset LastWritten = DateTimeOffset.UtcNow;
        public void UpdateWriteTime()
        {
            LastWritten = DateTimeOffset.UtcNow;
        }
        public bool     IsWrittenRecently(TimeSpan threshold)
        {
            return LastWritten > DateTimeOffset.UtcNow - threshold;
        }

    }
}
