namespace iChat.BackEnd.Services.Validators.TextMessageValidators
{
    public interface IBannedWordLoader
    {
        public List<string> GetBannedList();
    }
}
