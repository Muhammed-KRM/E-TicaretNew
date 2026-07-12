using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface IVitrinService
{
    Task<List<VitrinPackageDto>> GetPackagesAsync();
    Task PurchaseVitrinAsync(Guid listingId, int packageId, Guid userId);
}

