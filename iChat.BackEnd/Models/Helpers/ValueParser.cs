namespace iChat.BackEnd.Models.Helpers
{
    static public class ValueParser
    {
        private const long MinValidId = 1_000_000_000_000_000;

        public static bool TryLong(string input, out long id)
        {
            return long.TryParse(input, out id) && id > MinValidId;
        }
    }
}
