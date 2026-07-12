using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface IReviewService
{
    Task<List<ReviewDto>> GetByProductAsync(Guid productId);
    Task<ReviewDto> CreateAsync(ReviewCreateDto dto, Guid reviewerId);
    Task ApproveReviewAsync(Guid reviewId);
    Task DeleteReviewAsync(Guid reviewId, Guid userId);
}
