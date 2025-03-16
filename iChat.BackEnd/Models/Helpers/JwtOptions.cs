namespace iChat.BackEnd.Models.Helpers
{
    public class JwtOptions
    {
        public string SecretKey { get; set; }
        public int ExpireMinutes { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
    }
}
