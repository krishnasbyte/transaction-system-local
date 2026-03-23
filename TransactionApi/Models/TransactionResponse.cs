namespace TransactionApi.Models;

public class TransactionResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FailureReason { get; set; }
    
    // Helper method to create response from Transaction entity
    public static TransactionResponse FromTransaction(Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Status = transaction.Status,
            CreatedAt = transaction.CreatedAt,
            FailureReason = transaction.FailureReason
        };
    }
}
