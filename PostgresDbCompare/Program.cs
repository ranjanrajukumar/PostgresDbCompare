using PostgresDbCompare.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<CompareService>();

var app = builder.Build();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Compare}/{action=Index}/{id?}");

app.Run();