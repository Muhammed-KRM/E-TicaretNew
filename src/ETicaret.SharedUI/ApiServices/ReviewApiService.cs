using System.Net.Http.Json;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.SharedUI.ApiServices;

public class ReviewApiService : IReviewService
{
    private readonly HttpClient _http;

    public ReviewApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ReviewDto>> GetByProductAsync(Guid productId)
    {
        return await _http.GetFromJsonAsync<List<ReviewDto>>($"api/reviews/product/{productId}") ?? new List<ReviewDto>();
    }

    public async Task<ReviewDto> CreateAsync(ReviewCreateDto dto, Guid reviewerId)
    {
        var response = await _http.PostAsJsonAsync("api/reviews", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ReviewDto>() ?? new ReviewDto();
    }

    public Task ApproveReviewAsync(Guid reviewId)
    {
        throw new NotImplementedException("Yetkilendirme gerektiren Admin işlemi.");
    }
    
    public async Task DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        var response = await _http.DeleteAsync($"api/reviews/{reviewId}");
        response.EnsureSuccessStatusCode();
    }
}

