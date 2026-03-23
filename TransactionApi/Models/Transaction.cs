using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionApi.Models;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }           // Unique identifier
    
    [Required]
    public decimal Amount { get; set; }     // Transaction amount
    
    [Required]
    [StringLength(3)]
    public string Currency { get; set; }    // USD, EUR, NPR, etc.
    
    [Required]
    public string SourceAccount { get; set; }   // From account
    
    [Required]
    public string DestinationAccount { get; set; } // To account
    
    [Required]
    public string Status { get; set; }       // Pending, Processed, Failed
    
    public DateTime CreatedAt { get; set; }  // When transaction was created
    
    public DateTime? ProcessedAt { get; set; } // When it was processed (null if not yet)
    
    public string? FailureReason { get; set; } // If failed, why?
    
    // Constructor to set default values
    public Transaction()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = "Pending";
    }
}
