using Core.Extensions;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//Configure mongodb 
builder.Services.Configure<MongoSettings>(options =>
{
    options.ConnectionString = builder.Configuration.GetSection("ConnectionStrings:MongoDB:ConnectionString").Value;
    options.Database = builder.Configuration.GetSection("ConnectionStrings:MongoDB:Database").Value;
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
