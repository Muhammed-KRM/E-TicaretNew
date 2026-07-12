using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ETicaret.Business.DTOs;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Entities;
using ETicaret.Data.Repositories;

namespace ETicaret.Business.Services;

public class AddressManager : IAddressService
{
    private const string EC_GETMY     = "AM-001";
    private const string EC_ADD       = "AM-002";
    private const string EC_UPDATE    = "AM-003";
    private const string EC_DELETE    = "AM-004";
    private const string EC_SETDEF    = "AM-005";

    private readonly IRepository<Address> _addressRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IValidator<AddressCreateDto> _createValidator;
    private readonly ILogService _logService;

    public AddressManager(
        IRepository<Address> addressRepo,
        IRepository<User> userRepo,
        IValidator<AddressCreateDto> createValidator,
        ILogService logService)
    {
        _addressRepo = addressRepo;
        _userRepo = userRepo;
        _createValidator = createValidator;
        _logService = logService;
    }

    public async Task<List<AddressDto>> GetUserAddressesAsync(Guid userId)
    {
        try
        {
            var addresses = await _addressRepo.FindAsync(a => a.UserId == userId);
            
            var user = await _userRepo.GetByIdAsync(userId);
            
            return addresses.Select(a => new AddressDto(
                Id: a.Id,
                UserId: a.UserId,
                Title: a.Title,
                FullName: a.FullName,
                Phone: a.Phone,
                CityId: a.CityId,
                CityName: a.City?.Name ?? "",
                DistrictId: a.DistrictId,
                DistrictName: a.District?.Name ?? "",
                DetailedAddress: a.DetailedAddress,
                ZipCode: a.ZipCode,
                IsDefault: user?.DefaultAddressId == a.Id
            )).ToList();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETMY, ex, userId); throw; }
    }

    public async Task<AddressDto> AddAsync(AddressCreateDto dto, Guid userId)
    {
        try
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                throw new BusinessException(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));

            var address = new Address
            {
                UserId = userId,
                Title = dto.Title,
                FullName = dto.FullName,
                Phone = dto.Phone,
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                DetailedAddress = dto.DetailedAddress,
                ZipCode = dto.ZipCode,
                IsDefault = dto.IsDefault
            };

            await _addressRepo.AddAsync(address);
            await _addressRepo.SaveChangesAsync();

            if (dto.IsDefault)
            {
                await SetDefaultAsync(address.Id, userId);
            }

            // Return with populated City/District
            var savedAddress = await _addressRepo.GetByIdAsync(address.Id);

            return new AddressDto(
                Id: savedAddress!.Id,
                UserId: savedAddress.UserId,
                Title: savedAddress.Title,
                FullName: savedAddress.FullName,
                Phone: savedAddress.Phone,
                CityId: savedAddress.CityId,
                CityName: savedAddress.City?.Name ?? "",
                DistrictId: savedAddress.DistrictId,
                DistrictName: savedAddress.District?.Name ?? "",
                DetailedAddress: savedAddress.DetailedAddress,
                ZipCode: savedAddress.ZipCode,
                IsDefault: dto.IsDefault
            );
        }
        catch (BusinessException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_ADD, ex, dto, userId); throw; }
    }

    public async Task<AddressDto> UpdateAsync(Guid addressId, AddressUpdateDto dto, Guid userId)
    {
        try
        {
            var address = await _addressRepo.GetByIdAsync(addressId) ?? throw new NotFoundException("Adres", addressId);
            if (address.UserId != userId) throw new UnauthorizedException();

            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                throw new BusinessException(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));

            address.Title = dto.Title;
            address.FullName = dto.FullName;
            address.Phone = dto.Phone;
            address.CityId = dto.CityId;
            address.DistrictId = dto.DistrictId;
            address.DetailedAddress = dto.DetailedAddress;
            address.ZipCode = dto.ZipCode;

            _addressRepo.Update(address);
            await _addressRepo.SaveChangesAsync();

            if (dto.IsDefault)
            {
                await SetDefaultAsync(address.Id, userId);
            }

            var savedAddress = await _addressRepo.GetByIdAsync(address.Id);

            return new AddressDto(
                Id: savedAddress!.Id,
                UserId: savedAddress.UserId,
                Title: savedAddress.Title,
                FullName: savedAddress.FullName,
                Phone: savedAddress.Phone,
                CityId: savedAddress.CityId,
                CityName: savedAddress.City?.Name ?? "",
                DistrictId: savedAddress.DistrictId,
                DistrictName: savedAddress.District?.Name ?? "",
                DetailedAddress: savedAddress.DetailedAddress,
                ZipCode: savedAddress.ZipCode,
                IsDefault: dto.IsDefault
            );
        }
        catch (NotFoundException) { throw; }
        catch (UnauthorizedException) { throw; }
        catch (BusinessException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_UPDATE, ex, dto, userId); throw; }
    }

    public async Task DeleteAsync(Guid addressId, Guid userId)
    {
        try
        {
            var address = await _addressRepo.GetByIdAsync(addressId) ?? throw new NotFoundException("Adres", addressId);
            if (address.UserId != userId) throw new UnauthorizedException();

            _addressRepo.Delete(address);
            await _addressRepo.SaveChangesAsync();
        }
        catch (NotFoundException) { throw; }
        catch (UnauthorizedException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_DELETE, ex, addressId, userId); throw; }
    }

    public async Task SetDefaultAsync(Guid addressId, Guid userId)
    {
        try
        {
            var address = await _addressRepo.GetByIdAsync(addressId) ?? throw new NotFoundException("Adres", addressId);
            if (address.UserId != userId) throw new UnauthorizedException();

            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                user.DefaultAddressId = addressId;
                _userRepo.Update(user);
                await _userRepo.SaveChangesAsync();
            }
        }
        catch (NotFoundException) { throw; }
        catch (UnauthorizedException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_SETDEF, ex, addressId, userId); throw; }
    }
}
