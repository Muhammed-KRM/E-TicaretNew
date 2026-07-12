namespace ETicaret.Data.Entities;

public class Address
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int CityId { get; set; }
    public int DistrictId { get; set; }
    public string DetailedAddress { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public bool IsDefault { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public City City { get; set; } = null!;
    public District District { get; set; } = null!;
}
