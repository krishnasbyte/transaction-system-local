using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TransactionApi.Data;
using TransactionApi.Models;

namespace ProcessorService.Services;

public class TransactionProcessor : BackgroundService
{
    private readonly ILogger<TransactionProcessor> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TransactionProcessor(ILogger<TransactionProcessor> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Transaction Processor started at: {time}", DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

                var pendingTransactions = await dbContext.Transactions
                    .Where(t => t.Status == "Pending")
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var transaction in pendingTransactions)
                {
                    _logger.LogInformation("Processing transaction {TransactionId} for amount {Amount}", 
                        transaction.Id, transaction.Amount);
                    
                    await Task.Delay(2000, stoppingToken); // Simulate processing
                    
                    transaction.Status = "Processed";
                    transaction.ProcessedAt = DateTime.UtcNow;
                    _logger.LogInformation("Transaction {TransactionId} processed successfully", transaction.Id);
                }

                await dbContext.SaveChangesAsync(stoppingToken);
                await Task.Delay(5000, stoppingToken); // Wait 5 seconds before next check
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in transaction processing loop");
                await Task.Delay(10000, stoppingToken);
            }
        }

        _logger.LogInformation("Transaction Processor stopped at: {time}", DateTime.Now);
    }
}
