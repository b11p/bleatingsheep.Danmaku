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
                .WithOrigins("http://localhost:4000", "https://bleatingsheep.org")
                .WithHeaders("x-requested-with", "x-signalr-user-agent")
                .AllowCredentials();
        });
});

var app = builder.Build();

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

app.Run();
