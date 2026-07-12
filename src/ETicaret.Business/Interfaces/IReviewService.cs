using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface IReviewService
{
    Task<List<ReviewDto>> GetByListingAsync(Guid listingId);
    Task<ReviewDto> CreateAsync(ReviewCreateDto dto, Guid reviewerId);
    Task ApproveReviewAsync(Guid reviewId);
}

