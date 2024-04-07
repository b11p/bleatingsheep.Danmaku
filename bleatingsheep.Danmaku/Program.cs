using bleatingsheep.Danmaku.Hubs;
using bleatingsheep.Danmaku.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
// builder.Host.UseSystemd();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: "MyAllowSpecificOrigins",
        builder =>
        {
            _ = builder
                .WithOrigins("http://localhost:4000",
                    "http://localhost:8080",
                    "https://bleatingsheep.org",
                    "https://live.bleatingsheep.org",
                    "https://live.dihe.moe")
                .WithHeaders("x-requested-with", "x-signalr-user-agent")
                .AllowCredentials();
        });
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDanmakuPersistence, DanmakuPersistence>();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseCors("MyAllowSpecificOrigins");
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<DanmakuHub>("/danmakuHub");

app.Run();
