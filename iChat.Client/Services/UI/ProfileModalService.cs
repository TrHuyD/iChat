//using iChat.Client.Modals.User;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Forms;

//namespace iChat.Client.Services.UI;

//public class ProfileModalService
//{
//    public RenderFragment? CurrentModal { get; private set; }
//    public event Action? OnChange;

//    public void Show(RenderFragment modal)
//    {
//        CurrentModal = modal;
//        OnChange?.Invoke();
//    }

//    public void Close()
//    {
//        CurrentModal = null;
//        OnChange?.Invoke();
//    }

//    public void ShowDefaultModal(Func<Task> onSave)
//    {
//        Show(builder =>
//        {
//            builder.OpenComponent(0, typeof(ProfileModal));
//            builder.AddAttribute(1, "OnSave", EventCallback.Factory.Create<(string?, IBrowserFile?)>(this, onSave));
//            builder.AddAttribute(2, "OnClose", EventCallback.Factory.Create(this, Close));
//            builder.CloseComponent();
//        });
//    }
//}
