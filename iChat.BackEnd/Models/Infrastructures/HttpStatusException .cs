namespace iChat.BackEnd.Models.Infrastructures
{
    public class HttpStatusException : Exception
    {
        public int StatusCode { get; }
        public HttpStatusException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

}
