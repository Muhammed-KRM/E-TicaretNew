using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class UserApiService : IUserService
{
    private readonly HttpClient _http;
    public UserApiService(HttpClient http) => _http = http;
    public async Task<UserProfileDto> GetProfileAsync(Guid userId) => new UserProfileDto();
    public async Task UpdatePersonalInfoAsync(Guid userId, PersonalInfoDto dto) => await Task.CompletedTask;
    public async Task ChangePasswordAsync(Guid userId, PasswordChangeDto dto) => await Task.CompletedTask;
    public async Task UpdateNotificationSettingsAsync(Guid userId, NotificationSettingsDto dto) => await Task.CompletedTask;
}
