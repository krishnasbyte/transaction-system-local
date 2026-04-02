using System;
using TransactionApi.Models;
using Xunit;

namespace TransactionApi.Tests;

public class TransactionResponseTests
{
    [Fact]
    public void FromTransaction_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = 75.00m,
            Currency = "NPR",
            Status = "Processed",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            FailureReason = "Insufficient funds"
        };
        
        // Act
        var response = TransactionResponse.FromTransaction(transaction);
        
        // Assert
        Assert.Equal(transaction.Id, response.Id);
        Assert.Equal(transaction.Amount, response.Amount);
        Assert.Equal(transaction.Currency, response.Currency);
        Assert.Equal(transaction.Status, response.Status);
        Assert.Equal(transaction.CreatedAt, response.CreatedAt);
        Assert.Equal(transaction.FailureReason, response.FailureReason);
    }

    [Fact]
    public void FromTransaction_WithNullFailureReason_MapsToNull()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            FailureReason = null
        };
        
        // Act
        var response = TransactionResponse.FromTransaction(transaction);
        
        // Assert
        Assert.Null(response.FailureReason);
    }
}
