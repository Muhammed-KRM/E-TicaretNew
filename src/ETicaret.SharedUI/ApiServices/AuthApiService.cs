using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class AuthApiService : IAuthService
{
    private readonly HttpClient _http;

    public AuthApiService(HttpClient http)
    {
        _http = http;
    }

    private async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        var errorString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(errorString)) return "Beklenmeyen API hatası.";
        
        try
        {
            var document = System.Text.Json.JsonDocument.Parse(errorString);
            var root = document.RootElement;
            if (root.TryGetProperty("errorMessage", out var errMsg) && errMsg.ValueKind == System.Text.Json.JsonValueKind.String)
                return errMsg.GetString()!;
            
            if (root.TryGetProperty("errors", out var errorsProp))
            {
                var validationErrors = new List<string>();
                foreach (var err in errorsProp.EnumerateObject())
                {
                    if (err.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var msg in err.Value.EnumerateArray())
                            validationErrors.Add(msg.GetString()!);
                    }
                    else if (err.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        validationErrors.Add(err.Value.GetString()!);
                    }
                }
                if (validationErrors.Any()) return string.Join("<br/>", validationErrors);
            }

            if (root.TryGetProperty("detail", out var detailProp) && detailProp.ValueKind == System.Text.Json.JsonValueKind.String)
                return detailProp.GetString()!;

            if (root.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == System.Text.Json.JsonValueKind.String)
                return titleProp.GetString()!;
        }
        catch 
        { 
            return errorString; 
        }

        return errorString;
    }

    public async Task<AuthResultDto> RegisterAsync(UserRegisterDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
                return result ?? new AuthResultDto { Success = false, ErrorMessage = "Bağlantı hatası." };
            }
            return new AuthResultDto { Success = false, ErrorMessage = await ExtractErrorMessageAsync(response) };
        }
        catch (Exception ex)
        {
            return new AuthResultDto { Success = false, ErrorMessage = $"Ağ Hatası: {ex.Message}" };
        }
    }

    public async Task<AuthResultDto> LoginAsync(UserLoginDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
                return result ?? new AuthResultDto { Success = false, ErrorMessage = "Bağlantı hatası." };
            }
            return new AuthResultDto { Success = false, ErrorMessage = await ExtractErrorMessageAsync(response) };
        }
        catch (Exception ex)
        {
            return new AuthResultDto { Success = false, ErrorMessage = $"Ağ Hatası: {ex.Message}" };
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<UserDto>("api/auth/me");
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UserProfileUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/auth/profile", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>() ?? new UserDto();
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
    {
        var response = await _http.PostAsJsonAsync("api/auth/refresh-token", new RefreshTokenRequestDto { RefreshToken = refreshToken });
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResultDto>() ?? new AuthResultDto();
        }
        return new AuthResultDto { Success = false, ErrorMessage = "Token refresh failed" };
    }

    // Bu metot normalde back-end (Server) tarafında çağrılır, client API üzerinden bunu yapamaz
    public Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime)
    {
        throw new NotImplementedException("Bu metod Client API arayüzünden tetiklenemez.");
    }

    public Task SetUserStatusAsync(Guid userId, bool isActive)
    {
        throw new NotImplementedException("Bu metod Client API arayüzünden tetiklenemez.");
    }
}

