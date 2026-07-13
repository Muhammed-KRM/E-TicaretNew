using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ETicaret.Data.Entities;

namespace ETicaret.Data.Configurations;

/// <summary>
/// StockNotification entity için EF Core konfigürasyonu
/// </summary>
public class StockNotificationConfiguration : IEntityTypeConfiguration<StockNotification>
{
    public void Configure(EntityTypeBuilder<StockNotification> builder)
    {
        // Primary Key
        builder.HasKey(sn => sn.Id);
        
        // Indexes
        builder.HasIndex(sn => sn.ProductId)
            .HasDatabaseName("IX_StockNotification_ProductId");
        
        builder.HasIndex(sn => sn.Email)
            .HasDatabaseName("IX_StockNotification_Email");
            
        builder.HasIndex(sn => sn.IsNotified)
            .HasDatabaseName("IX_StockNotification_IsNotified");
        
        // Composite index: Aynı email aynı ürün için birden fazla bildirim talebi oluşturabilir (tarihsel kayıt için)
        builder.HasIndex(sn => new { sn.ProductId, sn.Email, sn.IsNotified })
            .HasDatabaseName("IX_StockNotification_Product_Email_Status");
        
        // Relationships
        
        // Product relationship
        builder.HasOne(sn => sn.Product)
            .WithMany()
            .HasForeignKey(sn => sn.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // Ürün silinince bildirim talepleri de silinir
        
        // Default values
        builder.Property(sn => sn.CreatedAt)
            .HasDefaultValueSql("NOW()");
            
        builder.Property(sn => sn.IsNotified)
            .HasDefaultValue(false);
    }
}
