using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ETicaret.Business.DTOs;
using ETicaret.Business.Events;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Helpers;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Entities;
using ETicaret.Data.Repositories;

namespace ETicaret.Business.Services;

public class ProductManager : IProductService
{
    private const string EC_SEARCH     = "PM-001";
    private const string EC_GETBYID    = "PM-002";
    private const string EC_GETBYSLUG  = "PM-003";
    private const string EC_GETBYCAT   = "PM-004";
    private const string EC_GETFEAT    = "PM-005";
    private const string EC_CREATE     = "PM-006";
    private const string EC_UPDATE     = "PM-007";
    private const string EC_DELETE     = "PM-008";
    private const string EC_GETMY      = "PM-009";

    private readonly IProductRepository _productRepo;
    private readonly ISearchService _searchService;
    private readonly IValidator<ProductCreateDto> _createValidator;
    private readonly IModerationService _moderationService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogService _logService;
    private readonly ICategoryRepository _categoryRepo;

    public ProductManager(
        IProductRepository productRepo,
        ISearchService searchService,
        IValidator<ProductCreateDto> createValidator,
        IModerationService moderationService,
        IPublishEndpoint publishEndpoint,
        ILogService logService,
        ICategoryRepository categoryRepo)
    {
        _productRepo = productRepo;
        _searchService = searchService;
        _createValidator = createValidator;
        _moderationService = moderationService;
        _publishEndpoint = publishEndpoint;
        _logService = logService;
        _categoryRepo = categoryRepo;
    }

    public async Task<ProductSearchResultDto> SearchAsync(ProductSearchFilterDto filters, CancellationToken ct = default)
    {
        try
        {
            return await _searchService.SearchAsync(filters);
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_SEARCH, ex, filters);
            throw;
        }
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(id); // WithSellerAndCategory dahil olmalı repoda
            if (product == null || !product.IsActive) return null;
            return MapToDto(product);
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETBYID, ex, id); throw; }
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        try
        {
            var product = await _productRepo.GetBySlugWithDetailsAsync(slug);
            if (product == null || !product.IsActive) return null;
            return MapToDto(product);
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETBYSLUG, ex, slug); throw; }
    }

    public async Task<List<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        try
        {
            var products = await _productRepo.GetByCategoryAsync(categoryId);
            return products.Select(MapToDto).ToList();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETBYCAT, ex, categoryId); throw; }
    }

    public async Task<List<ProductDto>> GetFeaturedAsync()
    {
        try
        {
            var products = await _productRepo.GetFeaturedProductsAsync(8);
            return products.Select(MapToDto).ToList();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETFEAT, ex); throw; }
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto, Guid userId)
    {
        try
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                throw new BusinessException(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));

            var category = (await _categoryRepo.FindAsync(c => c.Id == dto.CategoryId)).FirstOrDefault()
                ?? throw new NotFoundException("Kategori", dto.CategoryId);

            var cleanTitle = _moderationService.SanitizeHtml(dto.Title);
            var cleanDesc = _moderationService.SanitizeHtml(dto.Description);
            
            if (_moderationService.ContainsPII(cleanDesc) || _moderationService.ContainsInappropriateContent(cleanDesc))
                throw new BusinessException("Açıklama uygunsuz içerik veya kişisel veri (tel/email) barındırıyor.");

            string baseSlug = SlugHelper.GenerateSlug(cleanTitle);
            string uniqueSlug = await GenerateUniqueSlugAsync(baseSlug);

            var product = new Product
            {
                SellerId = userId,
                CategoryId = category.Id,
                Title = cleanTitle,
                Slug = uniqueSlug,
                Description = cleanDesc,
                Price = dto.Price,
                DiscountedPrice = dto.DiscountedPrice,
                StockQuantity = dto.StockQuantity,
                SKU = dto.SKU,
                Brand = dto.Brand,
                Weight = dto.Weight,
                IsActive = true
            };

            if (dto.ImageUrls.Any())
            {
                int order = 1;
                foreach (var url in dto.ImageUrls)
                {
                    product.Images.Add(new ProductImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }

            await _productRepo.AddAsync(product);
            await _productRepo.SaveChangesAsync();

            var savedProduct = await _productRepo.GetByIdAsync(product.Id); // Include'lu getirmek için
            var dtoResult = MapToDto(savedProduct ?? product);

            await _publishEndpoint.Publish(new ProductCreatedEvent { ProductId = product.Id });
            await _searchService.IndexProductAsync(dtoResult);

            return dtoResult;
        }
        catch (BusinessException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_CREATE, ex, dto, userId); throw; }
    }

    public async Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto dto, Guid userId)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(id) ?? throw new NotFoundException("Ürün", id);
            
            if (product.SellerId != userId)
                throw new UnauthorizedException();

            var cleanTitle = _moderationService.SanitizeHtml(dto.Title);
            var cleanDesc = _moderationService.SanitizeHtml(dto.Description);

            if (product.Title != cleanTitle)
            {
                string baseSlug = SlugHelper.GenerateSlug(cleanTitle);
                product.Slug = await GenerateUniqueSlugAsync(baseSlug, product.Id);
            }

            product.CategoryId = dto.CategoryId;
            product.Title = cleanTitle;
            product.Description = cleanDesc;
            product.Price = dto.Price;
            product.DiscountedPrice = dto.DiscountedPrice;
            product.StockQuantity = dto.StockQuantity;
            product.SKU = dto.SKU;
            product.Brand = dto.Brand;
            product.Weight = dto.Weight;
            product.IsActive = dto.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            product.Images.Clear();
            if (dto.ImageUrls.Any())
            {
                int order = 1;
                foreach (var url in dto.ImageUrls)
                {
                    product.Images.Add(new ProductImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }

            _productRepo.Update(product);
            await _productRepo.SaveChangesAsync();

            var savedProduct = await _productRepo.GetByIdAsync(product.Id);
            var dtoResult = MapToDto(savedProduct ?? product);

            await _publishEndpoint.Publish(new ProductUpdatedEvent { ProductId = product.Id });
            await _searchService.IndexProductAsync(dtoResult);

            return dtoResult;
        }
        catch (NotFoundException) { throw; }
        catch (UnauthorizedException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_UPDATE, ex, dto, userId); throw; }
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(id) ?? throw new NotFoundException("Ürün", id);
            
            if (product.SellerId != userId)
                throw new UnauthorizedException();

            _productRepo.Delete(product);
            await _productRepo.SaveChangesAsync();

            await _publishEndpoint.Publish(new ProductDeletedEvent { ProductId = id });
            await _searchService.DeleteProductIndexAsync(id);
        }
        catch (NotFoundException) { throw; }
        catch (UnauthorizedException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_DELETE, ex, id, userId); throw; }
    }

    public async Task<List<ProductDto>> GetMyProductsAsync(Guid userId)
    {
        try
        {
            var products = await _productRepo.FindAsync(p => p.SellerId == userId); // Detaylar eklenebilir
            return products.Select(MapToDto).ToList();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETMY, ex, userId); throw; }
    }

    private async Task<string> GenerateUniqueSlugAsync(string baseSlug, Guid? excludeId = null)
    {
        string slug = baseSlug;
        int counter = 1;
        while (true)
        {
            var exists = (await _productRepo.FindAsync(p => p.Slug == slug && (!excludeId.HasValue || p.Id != excludeId.Value))).Any();
            if (!exists) return slug;
            slug = $"{baseSlug}-{counter++}";
        }
    }

    private static ProductDto MapToDto(Product p) => new(
        Id: p.Id,
        SellerId: p.SellerId,
        SellerName: p.Seller?.FullName ?? "",
        CategoryId: p.CategoryId,
        CategoryName: p.Category?.Name ?? "",
        Title: p.Title,
        Slug: p.Slug,
        Description: p.Description,
        Price: p.Price,
        DiscountedPrice: p.DiscountedPrice,
        StockQuantity: p.StockQuantity,
        Brand: p.Brand,
        IsActive: p.IsActive,
        IsFeatured: p.IsFeatured,
        AverageRating: p.AverageRating,
        ReviewCount: p.ReviewCount,
        SalesCount: p.SalesCount,
        ImageUrls: p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
        CreatedAt: p.CreatedAt
    );
}
