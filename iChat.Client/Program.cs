using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using iChat.Client;
using iChat.Client.Services.Auth;
using Microsoft.AspNetCore.Components;
using iChat.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<JwtAuthHandler>();
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var tokenProvider = sp.GetRequiredService<TokenProvider>();

    return new HttpClient(new JwtAuthHandler(tokenProvider, navigationManager))
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});

builder.Services.AddScoped<UserStateService>();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
