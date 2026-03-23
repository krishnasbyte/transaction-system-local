using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransactionApi.Data;
using TransactionApi.Models;

namespace TransactionApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController : ControllerBase
{
    private readonly TransactionDbContext _context;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        TransactionDbContext context,
        ILogger<TransactionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/transactions
    // Returns all transactions with pagination
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting transactions page {Page} with size {PageSize}", page, pageSize);
        
        var transactions = await _context.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var response = transactions.Select(TransactionResponse.FromTransaction);
        
        return Ok(response);
    }

    // GET: api/transactions/{id}
    // Returns a specific transaction by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponse>> GetTransaction(Guid id)
    {
        _logger.LogInformation("Getting transaction with ID: {Id}", id);
        
        var transaction = await _context.Transactions.FindAsync(id);
        
        if (transaction == null)
        {
            _logger.LogWarning("Transaction with ID {Id} not found", id);
            return NotFound();
        }
        
        return Ok(TransactionResponse.FromTransaction(transaction));
    }

    // POST: api/transactions
    // Creates a new transaction
    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> CreateTransaction(TransactionRequest request)
    {
        _logger.LogInformation("Creating new transaction for amount {Amount} {Currency}", 
            request.Amount, request.Currency);
        
        // Create transaction entity from request
        var transaction = new Transaction
        {
            Amount = request.Amount,
            Currency = request.Currency,
            SourceAccount = request.SourceAccount,
            DestinationAccount = request.DestinationAccount,
            Status = "Pending"
        };
        
        // Add to database
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Transaction created with ID: {Id}", transaction.Id);
        
        // Return 201 Created with location header
        return CreatedAtAction(
            nameof(GetTransaction), 
            new { id = transaction.Id }, 
            TransactionResponse.FromTransaction(transaction));
    }

    // DELETE: api/transactions/{id}
    // Deletes a transaction (for testing/cleanup)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        _logger.LogInformation("Deleting transaction with ID: {Id}", id);
        
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }
        
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Transaction {Id} deleted", id);
        
        return NoContent();
    }

    // GET: api/transactions/health
    // Health check endpoint for Kubernetes
    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogDebug("Health check called");
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }
}
