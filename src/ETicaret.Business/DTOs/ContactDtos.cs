namespace ETicaret.Business.DTOs;

public class ContactInfoDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? WorkingHours { get; set; }
    public string? Description { get; set; }
    public string? MapEmbedUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
}

public class ContactMessageCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ContactMessageDto
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
    public DateTime CreatedAt { get; set; }
    public DateTime? RepliedAt { get; set; }
}

public class ContactReplyDto
{
    public string Reply { get; set; } = string.Empty;
}
