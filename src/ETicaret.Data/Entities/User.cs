using ETicaret.Data.Enums;

namespace ETicaret.Data.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.User;
    
    // AES-256 Şifreli veya null olabilir
    public string? PhoneEncrypted { get; set; }
    
    public string? ProfileImageUrl { get; set; }
    public decimal WalletBalance { get; set; }
    public Guid? DefaultAddressId { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; }
    // Profil Ayarları
    public DateTime? BirthDate { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool MarketingEmails { get; set; } = false;
    
    // Refresh Token for JWT
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Moderasyon alanları
    public int ViolationCount { get; set; } = 0;
    public DateTime? BannedUntil { get; set; }
    public DateTime? LastViolationAt { get; set; }
    public string? BanReason { get; set; }

    // Navigation Properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public Cart? Cart { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
