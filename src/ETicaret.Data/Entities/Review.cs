namespace ETicaret.Data.Entities;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ReviewerId { get; set; } // Yorumu yapan
    public Guid ProductId { get; set; } // Hangi ürüne yorum yapıldı
    public Guid? OrderId { get; set; } // Sadece satın alanlar yorum yapabilsin
    
    public int Rating { get; set; } // 1-5
    
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User Reviewer { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Order? Order { get; set; }
}
