namespace iChat.Client.Services.UI
{
    public class ToastService
    {
        public event Action<string, string>? OnShow;

        public void ShowToast(string message, string level)
        {
            OnShow?.Invoke(message, level);
        }
        public void ShowError(string message)
        {
            ShowToast(message, "danger");
        }
        public void ShowSuccess(string message)
        {
            ShowToast(message, "success");
        }
    }

}
