namespace ETicaret.Data.Entities;

/// <summary>
/// Site iletişim bilgileri (Tek kayıt — Admin tarafından güncellenir)
/// </summary>
public class ContactInfo
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? WorkingHours { get; set; }
    public string? Description { get; set; }
    public string? MapEmbedUrl { get; set; }         // Google Maps embed URL
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
