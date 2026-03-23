using Microsoft.EntityFrameworkCore;
using TransactionApi.Models;

namespace TransactionApi.Data;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options)
        : base(options)
    {
    }
    
    // This represents the Transactions table in the database
    public DbSet<Transaction> Transactions { get; set; }
    
    // Configure how the model maps to database
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2); // 18 digits total, 2 after decimal
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Status).HasMaxLength(20);
            
            // Create an index on Status for faster queries
            entity.HasIndex(e => e.Status);
            
            // Create an index on CreatedAt for faster date-based queries
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
