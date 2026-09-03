using DroneDeliveryApp.Core.Interfaces;
using DroneDeliveryApp.Core.Services;
using DroneDeliveryApp.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger / OpenAPI
builder.Services.AddSwaggerGen();

// Configure Health Checks
builder.Services.AddHealthChecks();

// Core Services
builder.Services.AddSingleton<ICsvParserService, CsvParserService>();
builder.Services.AddSingleton<IDeliveryPlanner, DeliveryPlanner>();

// Infrastructure Services (MongoDB with resilient fallback)
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS for local development flexibility
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Enable Swagger UI in Development & Production
app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Map Health Checks Endpoint
app.MapHealthChecks("/healthz");

app.MapControllers();

app.Run();
