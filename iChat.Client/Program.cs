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
#if DEBUG
using Microsoft.JSInterop;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped<FieldCssClassProvider, BootstrapFieldClassProvider>();
builder.Services.AddSingleton<ToastService>();




builder.Services.AddScoped<TokenProvider>();


builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});




builder.Services.AddScoped<JwtAuthHandler>(sp =>
{
    var tokenProvider = sp.GetRequiredService<TokenProvider>();
    var navigationManager = sp.GetRequiredService<NavigationManager>();
#if DEBUG
    var handler = new JwtAuthHandler(tokenProvider, navigationManager, sp.GetRequiredService<IJSRuntime>())
    {
        InnerHandler = new HttpClientHandler()
    };
#else
    var handler = new JwtAuthHandler(tokenProvider, navigationManager)
    {
        InnerHandler = new HttpClientHandler() 
    };
    
#endif
    return handler;
});


builder.Services.AddScoped<UserStateService>();

await builder.Build().RunAsync();
