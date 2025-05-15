namespace iChat.Client.Services.UI
{
    public class ToastService
    {
        public event Action<string, string>? OnShow;

        public void ShowToast(string message, string level)
        {
            OnShow?.Invoke(message, level);
        }
    }

}
