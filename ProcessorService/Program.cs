using Microsoft.EntityFrameworkCore;
using ProcessorService.Services;
using TransactionApi.Data;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=transactiondb;Username=admin;Password=MyP@ssw0rd";

builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHostedService<TransactionProcessor>();
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var host = builder.Build();
host.Run();
