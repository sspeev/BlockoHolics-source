using BlockoHolicsWeb.Data;
using BlockoHolicsWeb.Services;
using Timer = BlockoHolicsWeb.Services.Timer;
using Microsoft.EntityFrameworkCore;
using System.IO.Ports;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BlockoHolicsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IDbService, DbService>();

builder.Services.AddSingleton(_ =>
{
    return new SerialPort("COM3", 9600)
    {
        NewLine = "\n",
        ReadTimeout = 1000,
        WriteTimeout = 1000
    };
});

builder.Services.AddSingleton<Timer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<Timer>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}")
    .WithStaticAssets();

await app.RunAsync();
