using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ETicaret.Data.Entities;

namespace ETicaret.Data.Configurations;

/// <summary>
/// Refund entity için EF Core konfigürasyonu
/// </summary>
public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        // Primary Key
        builder.HasKey(r => r.Id);
        
        // Indexes
        builder.HasIndex(r => r.OrderId)
            .HasDatabaseName("IX_Refund_OrderId");
        
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_Refund_UserId");
            
        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_Refund_Status");
            
        builder.HasIndex(r => r.CreatedAt)
            .HasDatabaseName("IX_Refund_CreatedAt");
        
        // Relationships
        
        // Order relationship
        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict); // Sipariş silinirken iade kaydı korunur
            
        // User relationship
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirken iade kaydı korunur
            
        // ProcessedBy relationship
        builder.HasOne(r => r.ProcessedByUser)
            .WithMany()
            .HasForeignKey(r => r.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict); // Admin silinirken iade kaydı korunur
        
        // Default values
        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("NOW()");
            
        builder.Property(r => r.Status)
            .HasDefaultValue(RefundStatus.Pending);
    }
}
