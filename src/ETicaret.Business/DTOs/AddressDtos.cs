using System.ComponentModel.DataAnnotations;

namespace ETicaret.Business.DTOs;

public record AddressDto(
    Guid Id,
    Guid UserId,
    string Title,
    string FullName,
    string Phone,
    int CityId,
    string CityName,
    int DistrictId,
    string DistrictName,
    string DetailedAddress,
    string? ZipCode,
    bool IsDefault
);

public class AddressCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 81)]
    public int CityId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int DistrictId { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string DetailedAddress { get; set; } = string.Empty;
    
    public string? ZipCode { get; set; }
    public bool IsDefault { get; set; }
}

public class AddressUpdateDto : AddressCreateDto
{
}
