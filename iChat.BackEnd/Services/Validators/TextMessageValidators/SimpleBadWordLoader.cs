namespace iChat.BackEnd.Services.Validators.TextMessageValidators
{
    public class SimpleBadWordLoader :IBadWordLoarder
    {
        private readonly List<string> _badWords;

        public SimpleBadWordLoader()
        {
            _badWords = new List<string> { "shit" }; 
        }

        public List<string> GetBadWords()
        {
            return _badWords;
        }
    }
}
