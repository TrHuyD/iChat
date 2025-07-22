using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using iChat.Client;
using iChat.Client.Services.Auth;
using Microsoft.AspNetCore.Components;
using iChat.Client.Services;
using iChat.Client.Services.UI;
using iChat.Client.Services.Bootstrap;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using iChat.Client.Services.UserServices.ChatService;
using iChat.Client.Services.UserServices;
using Blazored.LocalStorage;
using TG.Blazor.IndexedDB;
using iChat.Client.Services.UserServices.Chat;
using iChat.Client.Services.UserServices.Chat.Util;
using iChat.Client.Services.UserServices.SignalR;








#if DEBUG
using Microsoft.JSInterop;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped<FieldCssClassProvider, BootstrapFieldClassProvider>();
builder.Services.AddSingleton<ToastService>();
builder.Services.AddScoped<UserMetadataService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<LastVisitedChannelService>();
builder.Services.AddScoped<LoginStateService>();
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<Lazy<UserStateService>>(provider => new Lazy<UserStateService>(() => provider.GetRequiredService<UserStateService>()));

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});




builder.Services.AddScoped<JwtAuthHandler>(sp =>
{
    var tokenProvider = sp.GetRequiredService<TokenProvider>();
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var configService = sp.GetRequiredService<ConfigService>();
    var handler = new JwtAuthHandler(tokenProvider, navigationManager,configService)
    {
        InnerHandler = new HttpClientHandler()
    };

    return handler;
});
builder.Services.AddIndexedDB(dbStore =>
{
    dbStore.DbName = "ChatDB";
    dbStore.Version = 1;

    dbStore.Stores.Add(new StoreSchema
    {
        Name = "Messages",
        PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false },
        Indexes = new List<IndexSpec>
        {
            new IndexSpec { Name = "roomId", KeyPath = "roomId", Auto = false, Unique = false },
            new IndexSpec { Name = "createdAt", KeyPath = "createdAt", Auto = false, Unique = false },
            new IndexSpec { Name = "messageId", KeyPath = "messageId", Auto = false, Unique = false }
        }
    });
});

builder.Services.AddScoped<MessageStorageService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<UserStateService>();
builder.Services.AddScoped<SignalRWorkerService>();
builder.Services.AddScoped<SignalRConnectionFactory>();
builder.Services.AddScoped<ChatSignalRClientService>();
builder.Services.AddScoped< ChatNavigationService>();
builder.Services.AddScoped<ChatMessageCacheService>();
builder.Services.AddScoped<InviteService>();
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddScoped<MessageHandleService>();
builder.Services.AddSingleton<ProfileModalService>();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
await builder.Build().RunAsync();
