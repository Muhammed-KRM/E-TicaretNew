namespace ETicaret.Data.Entities;

public class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }

    // Misafir kullanıcı için geçici ID
    public string? GuestId { get; set; }

    // Misafir iletişim bilgileri
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestPhone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
