using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface IAddressService
{
    Task<List<AddressDto>> GetUserAddressesAsync(Guid userId);
    Task<AddressDto> AddAsync(AddressCreateDto dto, Guid userId);
    Task<AddressDto> UpdateAsync(Guid id, AddressUpdateDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task SetDefaultAsync(Guid id, Guid userId);
}
