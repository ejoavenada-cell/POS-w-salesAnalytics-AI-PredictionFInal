using FoodOrderingSytemAIAnalytics.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using FoodOrderingSytemAIAnalytics.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register ApplicationDbContext with Hybrid Support
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Render typically provides DATABASE_URL
    var pgConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    // Check if we are running on Render
    bool isRender = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RENDER"));

    if (isRender && !string.IsNullOrEmpty(pgConnectionString))
    {
        Console.WriteLine(">>> CLOUD MODE: PostgreSQL Detected on Render");
        
        try 
        {
            // Parse the postgres://user:pass@host:port/database URL
            var uri = new Uri(pgConnectionString);
            var userInfo = uri.UserInfo.Split(':');

            var port = uri.Port <= 0 ? 5432 : uri.Port;
            var connStr = $"Host={uri.Host};" +
                          $"Port={port};" +
                          $"Username={userInfo[0]};" +
                          $"Password={userInfo[1]};" +
                          $"Database={uri.LocalPath.TrimStart('/')};" +
                          $"Ssl Mode=Require;Trust Server Certificate=true;";

            options.UseNpgsql(connStr);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> ERROR Parsing DATABASE_URL: {ex.Message}");
            // Fallback to trying the string as-is if it's not a URI
            options.UseNpgsql(pgConnectionString);
        }
    }
    else
    {
        Console.WriteLine(">>> LOCAL MODE: Using SQL Server");
        options.UseSqlServer(sqlConnectionString);
    }
});

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

// Add Session support for POS basket/state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Register Custom Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPOSService, POSService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<ITransactionIdGenerator, TransactionIdGenerator>();


var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Temporarily showing detailed errors in production to debug deployment
    app.UseDeveloperExceptionPage(); 
    app.UseHsts();
}
app.UseStaticFiles();

app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Auth/Login");
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
