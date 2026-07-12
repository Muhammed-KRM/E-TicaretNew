using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class AddressApiService : IAddressService
{
    private readonly HttpClient _http;

    public AddressApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AddressDto>> GetUserAddressesAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<List<AddressDto>>("api/addresses") ?? new List<AddressDto>();
    }

    public async Task<AddressDto?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _http.GetFromJsonAsync<AddressDto>($"api/addresses/{id}");
    }

    public async Task<AddressDto> AddAsync(AddressCreateDto dto, Guid userId)
    {
        var response = await _http.PostAsJsonAsync("api/addresses", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AddressDto>() ?? throw new Exception("Adres oluşturulamadı.");
    }

    public async Task<AddressDto> UpdateAsync(Guid id, AddressUpdateDto dto, Guid userId)
    {
        var response = await _http.PutAsJsonAsync($"api/addresses/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AddressDto>() ?? throw new Exception("Adres güncellenemedi.");
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var response = await _http.DeleteAsync($"api/addresses/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task SetDefaultAsync(Guid id, Guid userId)
    {
        var response = await _http.PostAsync($"api/addresses/{id}/set-default", null);
        response.EnsureSuccessStatusCode();
    }
}
