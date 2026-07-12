using System.ComponentModel.DataAnnotations;

namespace ETicaret.Business.DTOs;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewCreateDto
{
    [Required(ErrorMessage = "Ürün ID'si zorunludur.")]
    public Guid ProductId { get; set; }

    public Guid? OrderId { get; set; }

    [Required(ErrorMessage = "Puan zorunludur.")]
    [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Yorum içeriği zorunludur.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Yorum 10-1000 karakter arasında olmalıdır.")]
    public string Content { get; set; } = string.Empty;
}

public class ReviewUpdateDto
{
    [Required(ErrorMessage = "Puan zorunludur.")]
    [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Yorum içeriği zorunludur.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Yorum 10-1000 karakter arasında olmalıdır.")]
    public string Content { get; set; } = string.Empty;
}
