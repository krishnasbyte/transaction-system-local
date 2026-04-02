using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionApi.Controllers;
using TransactionApi.Data;
using TransactionApi.Models;
using Xunit;

namespace TransactionApi.Tests;

public class TransactionsControllerTests : IDisposable
{
    private readonly TransactionDbContext _context;
    private readonly TransactionsController _controller;
    private readonly Mock<ILogger<TransactionsController>> _loggerMock;

    public TransactionsControllerTests()
    {
        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new TransactionDbContext(options);
        _loggerMock = new Mock<ILogger<TransactionsController>>();
        _controller = new TransactionsController(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateTransaction_ValidRequest_ReturnsCreatedResponse()
    {
        // Arrange
        var request = new TransactionRequest
        {
            Amount = 100.50m,
            Currency = "USD",
            SourceAccount = "ACC123",
            DestinationAccount = "ACC456"
        };

        // Act
        var result = await _controller.CreateTransaction(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var transaction = Assert.IsType<TransactionResponse>(createdResult.Value);
        Assert.Equal(100.50m, transaction.Amount);
        Assert.Equal("USD", transaction.Currency);
        Assert.Equal("Pending", transaction.Status);
    }

    [Fact]
    public async Task CreateTransaction_ValidRequest_SavesToDatabase()
    {
        // Arrange
        var request = new TransactionRequest
        {
            Amount = 75.00m,
            Currency = "NPR",
            SourceAccount = "SOURCE123",
            DestinationAccount = "DEST456"
        };

        // Act
        await _controller.CreateTransaction(request);

        // Assert
        var saved = await _context.Transactions.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal(75.00m, saved.Amount);
        Assert.Equal("NPR", saved.Currency);
        Assert.Equal("Pending", saved.Status);
    }

    [Fact]
    public async Task GetTransaction_ExistingId_ReturnsTransaction()
    {
        // Arrange - create a test transaction with all required fields
        var transaction = new Transaction
        {
            Amount = 75.00m,
            Currency = "NPR",
            SourceAccount = "TEST123",
            DestinationAccount = "TEST456",
            Status = "Pending"
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTransaction(transaction.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTransaction = Assert.IsType<TransactionResponse>(okResult.Value);
        Assert.Equal(transaction.Id, returnedTransaction.Id);
        Assert.Equal(75.00m, returnedTransaction.Amount);
        Assert.Equal("NPR", returnedTransaction.Currency);
    }

    [Fact]
    public async Task GetTransaction_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetTransaction(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetTransactions_ReturnsPaginatedResults()
    {
        // Arrange - create 15 test transactions with all required fields
        for (int i = 0; i < 15; i++)
        {
            _context.Transactions.Add(new Transaction
            {
                Amount = i * 10,
                Currency = "USD",
                SourceAccount = $"ACC{i}",
                DestinationAccount = $"DST{i}",
                Status = "Pending"
            });
        }
        await _context.SaveChangesAsync();

        // Act - get first page (10 items)
        var result = await _controller.GetTransactions(page: 1, pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        // Convert to List to handle IEnumerable type
        var transactions = (okResult.Value as IEnumerable<TransactionResponse>)?.ToList();
        Assert.NotNull(transactions);
        Assert.Equal(10, transactions.Count);
    }

    [Fact]
    public async Task GetTransactions_SecondPage_ReturnsRemainingItems()
    {
        // Arrange - create 15 test transactions with all required fields
        for (int i = 0; i < 15; i++)
        {
            _context.Transactions.Add(new Transaction
            {
                Amount = i * 10,
                Currency = "USD",
                SourceAccount = $"ACC{i}",
                DestinationAccount = $"DST{i}",
                Status = "Pending"
            });
        }
        await _context.SaveChangesAsync();

        // Act - get second page (last 5 items)
        var result = await _controller.GetTransactions(page: 2, pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        // Convert to List to handle IEnumerable type
        var transactions = (okResult.Value as IEnumerable<TransactionResponse>)?.ToList();
        Assert.NotNull(transactions);
        Assert.Equal(5, transactions.Count);
    }

    [Fact]
    public async Task DeleteTransaction_ExistingId_ReturnsNoContent()
    {
        // Arrange - create a transaction with all required fields
        var transaction = new Transaction
        {
            Amount = 50.00m,
            Currency = "USD",
            SourceAccount = "DELETE123",
            DestinationAccount = "DELETE456",
            Status = "Pending"
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteTransaction(transaction.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        // Verify it's actually deleted
        var deleted = await _context.Transactions.FindAsync(transaction.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteTransaction_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteTransaction(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Health_ReturnsHealthyStatus()
    {
        // Act
        var result = _controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task MultipleTransactions_ArePaginatedCorrectly()
    {
        // Arrange - create 25 transactions
        for (int i = 0; i < 25; i++)
        {
            _context.Transactions.Add(new Transaction
            {
                Amount = i * 10,
                Currency = "USD",
                SourceAccount = $"SRC{i}",
                DestinationAccount = $"DST{i}",
                Status = "Pending"
            });
        }
        await _context.SaveChangesAsync();

        // Act - get page 1 (10 items)
        var page1 = await _controller.GetTransactions(page: 1, pageSize: 10);
        // Act - get page 2 (10 items)
        var page2 = await _controller.GetTransactions(page: 2, pageSize: 10);
        // Act - get page 3 (5 items)
        var page3 = await _controller.GetTransactions(page: 3, pageSize: 10);

        // Assert
        var page1Result = Assert.IsType<OkObjectResult>(page1.Result);
        var page1Transactions = (page1Result.Value as IEnumerable<TransactionResponse>)?.ToList();
        Assert.Equal(10, page1Transactions?.Count);
        
        var page2Result = Assert.IsType<OkObjectResult>(page2.Result);
        var page2Transactions = (page2Result.Value as IEnumerable<TransactionResponse>)?.ToList();
        Assert.Equal(10, page2Transactions?.Count);
        
        var page3Result = Assert.IsType<OkObjectResult>(page3.Result);
        var page3Transactions = (page3Result.Value as IEnumerable<TransactionResponse>)?.ToList();
        Assert.Equal(5, page3Transactions?.Count);
    }

    [Fact]
    public async Task Transactions_AreOrderedByCreatedAtDescending()
    {
        // Arrange - create transactions with different timestamps
        var transaction1 = new Transaction
        {
            Amount = 100,
            Currency = "USD",
            SourceAccount = "SRC1",
            DestinationAccount = "DST1",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        var transaction2 = new Transaction
        {
            Amount = 200,
            Currency = "USD",
            SourceAccount = "SRC2",
            DestinationAccount = "DST2",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };
        var transaction3 = new Transaction
        {
            Amount = 300,
            Currency = "USD",
            SourceAccount = "SRC3",
            DestinationAccount = "DST3",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Transactions.AddRange(transaction1, transaction2, transaction3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTransactions(page: 1, pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var transactions = (okResult.Value as IEnumerable<TransactionResponse>)?.ToList();
        
        Assert.NotNull(transactions);
        Assert.Equal(3, transactions.Count);
        // Most recent should be first
        Assert.Equal(300, transactions[0].Amount);
        Assert.Equal(200, transactions[1].Amount);
        Assert.Equal(100, transactions[2].Amount);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
