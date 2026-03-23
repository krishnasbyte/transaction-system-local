using System.ComponentModel.DataAnnotations;

namespace TransactionApi.Models;

public class TransactionRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; }
    
    [Required]
    public string SourceAccount { get; set; }
    
    [Required]
    public string DestinationAccount { get; set; }
}
