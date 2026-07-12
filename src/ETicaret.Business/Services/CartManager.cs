using Microsoft.EntityFrameworkCore;
using ETicaret.Business.DTOs;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Entities;
using ETicaret.Data.Repositories;

namespace ETicaret.Business.Services;

public class CartManager : ICartService
{
    private const string EC_GET       = "CM-001";
    private const string EC_ADD       = "CM-002";
    private const string EC_UPDATE    = "CM-003";
    private const string EC_REMOVE    = "CM-004";
    private const string EC_CLEAR     = "CM-005";
    private const string EC_COUNT     = "CM-006";

    private readonly IRepository<Cart> _cartRepo;
    private readonly IProductRepository _productRepo;
    private readonly ILogService _logService;

    public CartManager(IRepository<Cart> cartRepo, IProductRepository productRepo, ILogService logService)
    {
        _cartRepo = cartRepo;
        _productRepo = productRepo;
        _logService = logService;
    }

    public async Task<CartDto> GetCartAsync(Guid? userId, string? guestId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, guestId);
            return MapToDto(cart);
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GET, ex, userId); throw; }
    }

    public async Task AddItemAsync(Guid? userId, string? guestId, Guid productId, int quantity)
    {
        try
        {
            if (quantity <= 0) throw new BusinessException("Miktar 0'dan büyük olmalıdır.");

            var cart = await GetOrCreateCartAsync(userId, guestId);
            var product = await _productRepo.GetByIdAsync(productId) ?? throw new NotFoundException("Ürün", productId);

            if (!product.IsActive) throw new BusinessException("Bu ürün şu anda satışta değil.");
            
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            int newQuantity = (existingItem?.Quantity ?? 0) + quantity;

            if (product.StockQuantity < newQuantity)
                throw new OutOfStockException(productId, newQuantity);

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
            }
            else
            {
                cart.Items.Add(new CartItem { ProductId = productId, Quantity = quantity });
            }

            cart.LastModified = DateTime.UtcNow;
            _cartRepo.Update(cart);
            await _cartRepo.SaveChangesAsync();
        }
        catch (BusinessException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_ADD, ex, new { userId, productId, quantity }); throw; }
    }

    public async Task UpdateQuantityAsync(Guid? userId, string? guestId, Guid productId, int quantity)
    {
        try
        {
            if (quantity <= 0)
            {
                await RemoveItemAsync(userId, guestId, productId);
                return;
            }

            var cart = await GetOrCreateCartAsync(userId, guestId);
            var product = await _productRepo.GetByIdAsync(productId) ?? throw new NotFoundException("Ürün", productId);

            if (product.StockQuantity < quantity)
                throw new OutOfStockException(productId, quantity);

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                cart.LastModified = DateTime.UtcNow;
                _cartRepo.Update(cart);
                await _cartRepo.SaveChangesAsync();
            }
        }
        catch (BusinessException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_UPDATE, ex, new { userId, productId, quantity }); throw; }
    }

    public async Task RemoveItemAsync(Guid? userId, string? guestId, Guid productId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, guestId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem != null)
            {
                cart.Items.Remove(existingItem);
                cart.LastModified = DateTime.UtcNow;
                _cartRepo.Update(cart);
                await _cartRepo.SaveChangesAsync();
            }
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_REMOVE, ex, new { userId, productId }); throw; }
    }

    public async Task ClearCartAsync(Guid? userId, string? guestId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, guestId);
            cart.Items.Clear();
            cart.LastModified = DateTime.UtcNow;
            _cartRepo.Update(cart);
            await _cartRepo.SaveChangesAsync();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_CLEAR, ex, userId); throw; }
    }

    public async Task<int> GetCartItemCountAsync(Guid? userId, string? guestId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, guestId);
            return cart.Items.Sum(i => i.Quantity);
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_COUNT, ex, userId); throw; }
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid? userId, string? guestId)
    {
        if (userId == null && string.IsNullOrWhiteSpace(guestId))
            throw new BusinessException("Kullanıcı kimliği veya misafir kimliği bulunamadı.");

        Cart? cart = null;
        if (userId.HasValue)
        {
            cart = (await _cartRepo.FindAsync(c => c.UserId == userId)).FirstOrDefault();
        }
        else
        {
            cart = (await _cartRepo.FindAsync(c => c.GuestId == guestId)).FirstOrDefault();
        }

        if (cart == null)
        {
            cart = new Cart { UserId = userId, GuestId = guestId };
            await _cartRepo.AddAsync(cart);
            await _cartRepo.SaveChangesAsync();
        }
        return cart;
    }

    private static CartDto MapToDto(Cart c)
    {
        var items = c.Items.Select(i => new CartItemDto(
            ProductId: i.ProductId,
            ProductTitle: i.Product?.Title ?? "Silinmiş Ürün",
            ProductImageUrl: i.Product?.Images.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl,
            UnitPrice: i.Product?.DiscountedPrice ?? i.Product?.Price ?? 0,
            Quantity: i.Quantity,
            SubTotal: (i.Product?.DiscountedPrice ?? i.Product?.Price ?? 0) * i.Quantity,
            InStock: (i.Product?.StockQuantity ?? 0) >= i.Quantity
        )).ToList();

        return new CartDto(
            Items: items,
            TotalPrice: items.Sum(i => i.SubTotal),
            ItemCount: items.Sum(i => i.Quantity)
        );
    }
}
