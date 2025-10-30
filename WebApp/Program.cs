using Core.Extensions;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure MongoDB - support both configuration and environment variables
builder.Services.Configure<MongoSettings>(options =>
{
    // Try environment variables first, then fall back to configuration
    options.ConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") 
        ?? builder.Configuration.GetSection("ConnectionStrings:MongoDB:ConnectionString").Value 
        ?? throw new InvalidOperationException("MongoDB connection string not configured. Set MONGODB_CONNECTION_STRING environment variable or configure in appsettings.json");
    
    options.Database = Environment.GetEnvironmentVariable("MONGODB_DATABASE") 
        ?? builder.Configuration.GetSection("ConnectionStrings:MongoDB:Database").Value 
        ?? "DroneDelivery";
});
builder.Services.AddSingleton<MongoSettings>();

// Inject services
builder.Services.UseInfrastructurePersistence();
builder.Services.UseCoreServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
