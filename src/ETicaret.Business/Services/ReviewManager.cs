using Microsoft.EntityFrameworkCore;
using ETicaret.Business.DTOs;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using ETicaret.Data.Repositories;
using ETicaret.Data.Enums;

namespace ETicaret.Business.Services;

public class ReviewManager : IReviewService
{
    private const string EC_GETBYPRODUCT = "RM-001";
    private const string EC_CREATE       = "RM-002";
    private const string EC_APPROVE      = "RM-003";
    private const string EC_DELETE       = "RM-004";

    private readonly AppDbContext _context;
    private readonly IProductRepository _productRepo;
    private readonly ILogService _logService;

    public ReviewManager(AppDbContext context, IProductRepository productRepo, ILogService logService)
    {
        _context = context;
        _productRepo = productRepo;
        _logService = logService;
    }

    public async Task<List<ReviewDto>> GetByProductAsync(Guid productId)
    {
        try
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToDto(r))
                .ToListAsync();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETBYPRODUCT, ex, productId); throw; }
    }

    public async Task<ReviewDto> CreateAsync(ReviewCreateDto dto, Guid reviewerId)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId)
                ?? throw new NotFoundException("Ürün", dto.ProductId);


            // Kullanıcı bu ürünü satın almış mı kontrolü
            var hasPurchased = await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.ProductId == dto.ProductId && oi.Order.UserId == reviewerId && oi.Order.Status == OrderStatus.Delivered);

            if (!hasPurchased)
                throw new BusinessException("Sadece satın aldığınız ve teslim edilmiş ürünlere yorum yapabilirsiniz.");

            // Daha önce yorum yapmış mı?
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == dto.ProductId && r.ReviewerId == reviewerId);

            if (hasReviewed)
                throw new BusinessException("Bu ürüne zaten yorum yaptınız.");

            var review = new Review
            {
                ReviewerId = reviewerId,
                ProductId = dto.ProductId,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Content = dto.Content,
                IsApproved = true
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Product AverageRating ve ReviewCount güncelle
            var allReviews = await _context.Reviews
                .Where(r => r.ProductId == dto.ProductId && r.IsApproved)
                .ToListAsync();

            product.AverageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0;
            product.ReviewCount = allReviews.Count;
            _productRepo.Update(product);
            await _productRepo.SaveChangesAsync();

            var saved = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            return MapToDto(saved ?? review);
        }
        catch (BusinessException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_CREATE, ex, dto, reviewerId); throw; }
    }

    public async Task ApproveReviewAsync(Guid reviewId)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(reviewId)
                ?? throw new NotFoundException("Yorum", reviewId);
            review.IsApproved = true;
            await _context.SaveChangesAsync();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_APPROVE, ex, reviewId); throw; }
    }

    public async Task DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(reviewId)
                ?? throw new NotFoundException("Yorum", reviewId);

            if (review.ReviewerId != userId)
                throw new UnauthorizedException();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Ürün rating güncelle
            var product = await _productRepo.GetByIdAsync(review.ProductId);
            if (product != null)
            {
                var allReviews = await _context.Reviews
                    .Where(r => r.ProductId == product.Id && r.IsApproved)
                    .ToListAsync();

                product.AverageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0;
                product.ReviewCount = allReviews.Count;
                _productRepo.Update(product);
                await _productRepo.SaveChangesAsync();
            }
        }
        catch (NotFoundException) { throw; }
        catch (UnauthorizedException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_DELETE, ex, reviewId, userId); throw; }
    }

    private static ReviewDto MapToDto(Review r) => new()
    {
        Id = r.Id,
        ReviewerId = r.ReviewerId,
        ReviewerName = r.Reviewer?.FullName ?? "Kullanıcı",
        ProductId = r.ProductId,
        ProductTitle = r.Product?.Title ?? "",
        OrderId = r.OrderId,
        Rating = r.Rating,
        Content = r.Content,
        IsApproved = r.IsApproved,
        CreatedAt = r.CreatedAt
    };
}
