using Microsoft.EntityFrameworkCore;
using ETicaret.Data.Entities;

namespace ETicaret.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();

    public DbSet<City> Cities => Set<City>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();
    public DbSet<EndpointLog> EndpointLogs => Set<EndpointLog>();
    public DbSet<FunctionLog> FunctionLogs => Set<FunctionLog>();
    public DbSet<ViolationLog> ViolationLogs => Set<ViolationLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ContactInfo> ContactInfo => Set<ContactInfo>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Entity framework konfigürasyon dosyalarını (IEntityTypeConfiguration) otomatik tarayıp ekle
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
