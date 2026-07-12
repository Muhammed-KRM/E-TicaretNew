using MassTransit;
using Microsoft.EntityFrameworkCore;
using ETicaret.Business.DTOs;
using ETicaret.Business.Events;
using ETicaret.Business.Exceptions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Entities;
using ETicaret.Data.Enums;
using ETicaret.Data.Repositories;
using ETicaret.Data.Context; // transaction için

namespace ETicaret.Business.Services;

public class OrderManager : IOrderService
{
    private const string EC_CREATE    = "OM-001";
    private const string EC_GET       = "OM-002";
    private const string EC_MYORDERS  = "OM-003";
    private const string EC_CANCEL    = "OM-004";

    private readonly AppDbContext _context; // transaction ve karmaşık sorgular için
    private readonly ICartService _cartService;
    private readonly IProductRepository _productRepo;
    private readonly IRepository<Address> _addressRepo;
    private readonly ICouponService _couponService;
    private readonly IPaymentService _paymentService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogService _logService;
    private readonly ISettingService _settingService;

    public OrderManager(
        AppDbContext context,
        ICartService cartService,
        IProductRepository productRepo,
        IRepository<Address> addressRepo,
        ICouponService couponService,
        IPaymentService paymentService,
        IPublishEndpoint publishEndpoint,
        ILogService logService,
        ISettingService settingService)
    {
        _context = context;
        _cartService = cartService;
        _productRepo = productRepo;
        _addressRepo = addressRepo;
        _couponService = couponService;
        _paymentService = paymentService;
        _publishEndpoint = publishEndpoint;
        _logService = logService;
        _settingService = settingService;
    }

    public async Task<OrderResultDto> CreateOrderAsync(Guid userId, OrderCreateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cart = await _cartService.GetCartAsync(userId);
            if (!cart.Items.Any()) throw new BusinessException("Sepetiniz boş.");

            var shippingAddress = await _addressRepo.GetByIdAsync(dto.ShippingAddressId) ?? throw new NotFoundException("Teslimat Adresi", dto.ShippingAddressId);
            if (shippingAddress.UserId != userId) throw new UnauthorizedException();

            var billingAddress = await _addressRepo.GetByIdAsync(dto.BillingAddressId) ?? throw new NotFoundException("Fatura Adresi", dto.BillingAddressId);
            if (billingAddress.UserId != userId) throw new UnauthorizedException();

            decimal subTotal = cart.TotalPrice;
            decimal shippingCost = await _settingService.GetDecimalSettingAsync("DefaultShippingPrice", 50m);
            decimal freeShippingThreshold = await _settingService.GetDecimalSettingAsync("FreeShippingThreshold", 500m);
            
            if (subTotal >= freeShippingThreshold) shippingCost = 0;

            decimal discountAmount = 0;
            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var coupon = await _couponService.ValidateCouponAsync(dto.CouponCode, subTotal);
                if (coupon.IsValid)
                {
                    discountAmount = coupon.DiscountAmount;
                }
            }

            decimal totalPrice = subTotal + shippingCost - discountAmount;
            string orderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(100, 999);

            var order = new Order
            {
                UserId = userId,
                OrderNumber = orderNumber,
                ShippingAddressId = shippingAddress.Id,
                BillingAddressId = billingAddress.Id,
                ShippingAddressSnapshot = $"{shippingAddress.Title} - {shippingAddress.DetailedAddress}, {shippingAddress.DistrictId}/{shippingAddress.CityId}",
                BillingAddressSnapshot = $"{billingAddress.Title} - {billingAddress.DetailedAddress}, {billingAddress.DistrictId}/{billingAddress.CityId}",
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                ShippingCompany = "Aras Kargo", // Örnek
                SubTotal = subTotal,
                ShippingPrice = shippingCost,
                DiscountAmount = discountAmount,
                CouponCode = string.IsNullOrWhiteSpace(dto.CouponCode) ? null : dto.CouponCode,
                TotalPrice = totalPrice,
                CustomerNote = dto.CustomerNote,
                IpAddress = dto.IpAddress ?? "Unknown",
                UserAgent = dto.UserAgent ?? "Unknown"
            };

            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId) ?? throw new NotFoundException("Ürün", item.ProductId);
                if (product.StockQuantity < item.Quantity)
                    throw new OutOfStockException(product.Id, item.Quantity);

                // Stok düş
                product.StockQuantity -= item.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductTitle,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.SubTotal
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Ödeme isteği oluştur
            var user = await _context.Users.FindAsync(userId);
            var paymentRequest = new PaymentRequest
            {
                UserId = userId,
                OrderId = order.Id,
                Amount = totalPrice,
                ReturnUrl = dto.ReturnUrl,
                BuyerEmail = user?.Email,
                BuyerName = user?.FullName,
                BuyerIp = dto.IpAddress,
                BasketItems = cart.Items.Select(i => new PaymentBasketItem
                {
                    Id = i.ProductId.ToString(),
                    Name = i.ProductTitle,
                    Category = "Product", // Kategori bilgisi DTO'da eklenebilir
                    Price = i.UnitPrice
                }).ToList()
            };

            var paymentResult = await _paymentService.ProcessPaymentAsync(paymentRequest);

            if (!paymentResult.Success)
            {
                throw new BusinessException($"Ödeme başlatılamadı: {paymentResult.ErrorMessage}");
            }

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                await _couponService.UseCouponAsync(dto.CouponCode, userId, order.Id);
            }

            await _cartService.ClearCartAsync(userId);
            await transaction.CommitAsync();

            return new OrderResultDto(
                Success: true,
                OrderId: order.Id,
                OrderNumber: order.OrderNumber,
                PaymentUrl: paymentResult.RedirectUrl ?? ""
            );
        }
        catch (BusinessException) { await transaction.RollbackAsync(); throw; }
        catch (Exception ex) 
        { 
            await transaction.RollbackAsync(); 
            await _logService.LogFunctionErrorAsync(EC_CREATE, ex, dto, userId); 
            throw; 
        }
    }

    public async Task<OrderDto?> GetOrderAsync(Guid orderId, Guid userId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return null;

            return MapToDto(order);
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GET, ex, orderId, userId); throw; }
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync(Guid userId)
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_MYORDERS, ex, userId); throw; }
    }

    public async Task CancelOrderAsync(Guid orderId, Guid userId)
    {
        try
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId) 
                ?? throw new NotFoundException("Sipariş", orderId);

            if (order.UserId != userId) throw new UnauthorizedException();
            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
                throw new BusinessException("Kargoya verilmiş veya teslim edilmiş siparişler iptal edilemez.");

            order.Status = OrderStatus.Cancelled;
            
            // Eğer ödeme alınmışsa iade (Refund) işlemi yapılacak (Ödeme sağlayıcı entegrasyonuyla)
            // if (order.PaymentStatus == PaymentStatus.Paid) { await _paymentService.RefundAsync(...); }
            
            // Stokları geri ver
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null) product.StockQuantity += item.Quantity;
            }

            await _context.SaveChangesAsync();

            await _publishEndpoint.Publish(new OrderStatusChangedEvent
            {
                OrderId = order.Id,
                NewStatus = OrderStatus.Cancelled
            });
        }
        catch (BusinessException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_CANCEL, ex, orderId, userId); throw; }
    }

    private static OrderDto MapToDto(Order o) => new(
        Id: o.Id,
        OrderNumber: o.OrderNumber,
        Status: o.Status.ToString(),
        PaymentStatus: o.PaymentStatus.ToString(),
        CreatedAt: o.CreatedAt,
        SubTotal: o.SubTotal,
        ShippingCost: o.ShippingPrice,
        DiscountAmount: o.DiscountAmount,
        TotalPrice: o.TotalPrice,
        TrackingNumber: o.TrackingNumber,
        Items: o.Items.Select(i => new OrderItemDto(
            ProductId: i.ProductId,
            ProductName: i.ProductName,
            ProductImageUrl: i.Product?.Images.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl,
            UnitPrice: i.UnitPrice,
            Quantity: i.Quantity,
            TotalPrice: i.TotalPrice
        )).ToList()
    );
}
