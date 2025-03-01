using iChat.App.Infrastructure.HttpHandlers;
using iChat.App.Models.Helper;
using iChat.App.Services.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient(HttpClientType.AuthApiClient).AddHttpMessageHandler<JwtDelegatingHandler>();
builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ApiAddressSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddTransient<JwtDelegatingHandler>();
builder.Services.AddTransient<IAuthService, AuthService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
