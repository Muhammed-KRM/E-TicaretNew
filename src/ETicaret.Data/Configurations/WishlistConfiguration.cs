using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ETicaret.Data.Entities;

namespace ETicaret.Data.Configurations;

/// <summary>
/// Wishlist entity için EF Core konfigürasyonu
/// </summary>
public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        // Primary Key
        builder.HasKey(w => w.Id);
        
        // Indexes
        builder.HasIndex(w => w.UserId)
            .HasDatabaseName("IX_Wishlist_UserId");
        
        builder.HasIndex(w => w.ProductId)
            .HasDatabaseName("IX_Wishlist_ProductId");
            
        // Unique constraint: Bir kullanıcı aynı ürünü birden fazla kez favoriye ekleyemez
        builder.HasIndex(w => new { w.UserId, w.ProductId })
            .IsUnique()
            .HasDatabaseName("UQ_Wishlist_User_Product");
        
        // Relationships
        
        // User relationship
        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinince favorileri de silinir
            
        // Product relationship
        builder.HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // Ürün silinince favorilerden de silinir
        
        // Default values
        builder.Property(w => w.CreatedAt)
            .HasDefaultValueSql("NOW()");
    }
}
