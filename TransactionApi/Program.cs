using Microsoft.EntityFrameworkCore;
using TransactionApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL
// We'll get connection string from environment or appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=transactiondb;Username=admin;Password=MyP@ssw0rd";

builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Create database and apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    dbContext.Database.EnsureCreated(); // Creates database if it doesn't exist
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => "Transaction API is running! Visit /swagger for API documentation");

app.Run();
