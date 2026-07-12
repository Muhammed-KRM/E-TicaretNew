using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class ContactApiService : IContactService
{
    private readonly HttpClient _http;
    public ContactApiService(HttpClient http) => _http = http;

    public async Task<ContactInfoDto> GetContactInfoAsync()
    {
        try { return await _http.GetFromJsonAsync<ContactInfoDto>("api/contact/info") ?? new ContactInfoDto(); }
        catch { return new ContactInfoDto(); }
    }

    public async Task SendMessageAsync(ContactMessageCreateDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/contact/message", dto);
            response.EnsureSuccessStatusCode();
        }
        catch { }
    }

    public async Task UpdateContactInfoAsync(ContactInfoDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync("api/admin/contact/info", dto);
            response.EnsureSuccessStatusCode();
        }
        catch { }
    }

    public async Task<List<ContactMessageDto>> GetMessagesAsync(bool? isRead = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = $"api/admin/contact/messages?page={page}&pageSize={pageSize}";
            if (isRead.HasValue) query += $"&isRead={isRead}";
            return await _http.GetFromJsonAsync<List<ContactMessageDto>>(query) ?? new();
        }
        catch { return new(); }
    }

    public async Task MarkAsReadAsync(int messageId)
    {
        try
        {
            var response = await _http.PostAsync($"api/admin/contact/messages/{messageId}/read", null);
            response.EnsureSuccessStatusCode();
        }
        catch { }
    }

    public async Task ReplyToMessageAsync(int messageId, string reply)
    {
        try
        {
            var dto = new ContactReplyDto { Reply = reply };
            var response = await _http.PostAsJsonAsync($"api/admin/contact/messages/{messageId}/reply", dto);
            response.EnsureSuccessStatusCode();
        }
        catch { }
    }
}
