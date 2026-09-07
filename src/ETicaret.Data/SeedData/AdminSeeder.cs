using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using ETicaret.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Data.SeedData;

/// <summary>
/// Admin kullanıcı ve test verileri oluşturur
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAdminUserAsync(AppDbContext context)
    {
        // Admin kullanıcısı zaten var mı kontrol et
        var adminExists = await context.Users.AnyAsync(u => u.Email == "admin@eticaret.com");
        
        if (!adminExists)
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@eticaret.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Güvenli şifre hashleme
                FullName = "Admin User",
                Role = UserRole.Admin,
                IsActive = true,
                IsEmailVerified = true,
                EmailNotifications = true,
                MarketingEmails = false,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
            
            Console.WriteLine("✅ Admin kullanıcısı oluşturuldu!");
            Console.WriteLine($"   Email: admin@eticaret.com");
            Console.WriteLine($"   Şifre: Admin123!");
        }
        else
        {
            Console.WriteLine("ℹ️  Admin kullanıcısı zaten mevcut.");
        }
    }

    public static async Task SeedTestUserAsync(AppDbContext context)
    {
        // Test kullanıcısı zaten var mı kontrol et
        var testUserExists = await context.Users.AnyAsync(u => u.Email == "test@test.com");
        
        if (!testUserExists)
        {
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                FullName = "Test User",
                Role = UserRole.User,
                IsActive = true,
                IsEmailVerified = true,
                EmailNotifications = true,
                MarketingEmails = true,
                WalletBalance = 1000, // Test için başlangıç bakiyesi
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(testUser);
            
            // Test kullanıcısı için sepet oluştur
            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            await context.Carts.AddAsync(cart);
            
            await context.SaveChangesAsync();
            
            Console.WriteLine("✅ Test kullanıcısı oluşturuldu!");
            Console.WriteLine($"   Email: test@test.com");
            Console.WriteLine($"   Şifre: Test123!");
        }
        else
        {
            Console.WriteLine("ℹ️  Test kullanıcısı zaten mevcut.");
        }
    }

    public static async Task SeedAllAsync(AppDbContext context)
    {
        Console.WriteLine("🌱 Seed data başlatılıyor...");
        
        await SeedAdminUserAsync(context);
        await SeedTestUserAsync(context);
        
        Console.WriteLine("✅ Seed data tamamlandı!");
    }
}
