using OrderApi.Core.Interfaces;
using OrderApi.Infrastructure.Repositories;
using OrderApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection
// Connection string would typically come from appsettings.json or Environment Variables
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    // Fallback or throw, but for docker compose we usually set env var
    // For now we can allow it to be null if we assume it will be present at runtime
    // or log a warning.
    Console.WriteLine("Warning: Connection string is null configurations.");
}

builder.Services.AddScoped<IOrderRepository>(sp => new OrderRepository(connectionString!));
builder.Services.AddScoped<ILogisticsGateway, LogisticsGateway>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Disable for internal docker communication simplicity or configure certs

app.UseAuthorization();

app.MapControllers();

app.Run();
