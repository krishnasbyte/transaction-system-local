using System;
using TransactionApi.Models;
using Xunit;

namespace TransactionApi.Tests;

public class TransactionModelTests
{
    [Fact]
    public void Transaction_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var transaction = new Transaction();
        
        // Assert
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal("Pending", transaction.Status);
        Assert.True(transaction.CreatedAt <= DateTime.UtcNow);
        Assert.Null(transaction.ProcessedAt);
        Assert.Null(transaction.FailureReason);
    }

    [Fact]
    public void Transaction_CanSetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddHours(-1);
        
        // Act
        var transaction = new Transaction
        {
            Id = id,
            Amount = 100.50m,
            Currency = "USD",
            SourceAccount = "SOURCE123",
            DestinationAccount = "DEST456",
            Status = "Processed",
            CreatedAt = createdAt,
            ProcessedAt = DateTime.UtcNow,
            FailureReason = null
        };
        
        // Assert
        Assert.Equal(id, transaction.Id);
        Assert.Equal(100.50m, transaction.Amount);
        Assert.Equal("USD", transaction.Currency);
        Assert.Equal("SOURCE123", transaction.SourceAccount);
        Assert.Equal("DEST456", transaction.DestinationAccount);
        Assert.Equal("Processed", transaction.Status);
        Assert.Equal(createdAt, transaction.CreatedAt);
        Assert.NotNull(transaction.ProcessedAt);
    }

    [Fact]
    public void Transaction_StatusDefaultIsPending()
    {
        // Arrange & Act
        var transaction = new Transaction();
        
        // Assert
        Assert.Equal("Pending", transaction.Status);
    }
}
