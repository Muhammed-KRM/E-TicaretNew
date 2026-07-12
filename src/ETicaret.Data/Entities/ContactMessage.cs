namespace ETicaret.Data.Entities;

/// <summary>
/// Kullanıcıların gönderdiği iletişim formu mesajları
/// </summary>
public class ContactMessage
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public string? AdminReply { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RepliedAt { get; set; }
}
