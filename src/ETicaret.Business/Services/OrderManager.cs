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

    public async Task<OrderResultDto> CreateOrderAsync(Guid? userId, string? guestId, OrderCreateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cart = await _cartService.GetCartAsync(userId, guestId);
            if (!cart.Items.Any()) throw new BusinessException("Sepetiniz boş.");

            var shippingAddress = await _addressRepo.GetByIdAsync(dto.ShippingAddressId) ?? throw new NotFoundException("Teslimat Adresi", dto.ShippingAddressId);
            if (userId.HasValue && shippingAddress.UserId != userId.Value) throw new UnauthorizedException();

            var billingAddress = await _addressRepo.GetByIdAsync(dto.BillingAddressId) ?? throw new NotFoundException("Fatura Adresi", dto.BillingAddressId);
            if (userId.HasValue && billingAddress.UserId != userId.Value) throw new UnauthorizedException();

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

            if (!string.IsNullOrWhiteSpace(dto.CouponCode) && userId.HasValue)
            {
                await _couponService.UseCouponAsync(dto.CouponCode, userId.Value, order.Id);
            }

            await _cartService.ClearCartAsync(userId, guestId);
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

    public async Task<OrderDto?> GetOrderAsync(Guid orderId, Guid? userId, string? guestId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && (!userId.HasValue || o.UserId == userId.Value));

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

    public async Task CancelOrderAsync(Guid orderId, Guid? userId, string? guestId)
    {
        try
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId) 
                ?? throw new NotFoundException("Sipariş", orderId);

            if (userId.HasValue && order.UserId != userId.Value) throw new UnauthorizedException();
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
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_CANCEL, ex, orderId); throw; }
    }

    public async Task RequestReturnAsync(Guid orderId, Guid? userId, string? guestId, string returnReason)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && (!userId.HasValue || o.UserId == userId.Value))
            ?? throw new NotFoundException("Sipariş", orderId);

        if (order.Status != OrderStatus.Delivered)
            throw new BusinessException("Yalnızca teslim edilmiş siparişler için iade talebi oluşturulabilir.");

        if (order.DeliveredAt.HasValue && (DateTime.UtcNow - order.DeliveredAt.Value).TotalDays > 14)
            throw new BusinessException("İade süresi (14 gün) dolmuştur.");

        order.Status = OrderStatus.ReturnRequested;
        order.ReturnReason = returnReason;
        order.ReturnRequestedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ApproveReturnAsync(Guid orderId, string? adminNote)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException("Sipariş", orderId);

        if (order.Status != OrderStatus.ReturnRequested)
            throw new BusinessException("Bu sipariş için iade talebi bulunmuyor.");

        if (!string.IsNullOrEmpty(order.PaymentTransactionId))
        {
            var refundSuccess = await _paymentService.RefundAsync(
                order.PaymentTransactionId, order.TotalPrice);
            if (!refundSuccess)
                throw new BusinessException("Ödeme iadesi başarısız oldu. Lütfen tekrar deneyin.");
        }

        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                product.SalesCount -= item.Quantity;
            }
        }

        order.Status = OrderStatus.Refunded;
        order.RefundedAt = DateTime.UtcNow;
        order.RefundAmount = order.TotalPrice;
        order.AdminReturnNote = adminNote;
        await _context.SaveChangesAsync();
    }

    public async Task RejectReturnAsync(Guid orderId, string adminNote)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException("Sipariş", orderId);

        if (order.Status != OrderStatus.ReturnRequested)
            throw new BusinessException("Bu sipariş için iade talebi bulunmuyor.");

        order.Status = OrderStatus.ReturnRejected;
        order.AdminReturnNote = adminNote;
        await _context.SaveChangesAsync();
    }

    public async Task<List<OrderDto>> GetReturnRequestsAsync()
    {
        return await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .Where(o => o.Status == OrderStatus.ReturnRequested)
            .OrderByDescending(o => o.ReturnRequestedAt)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task UpdateShippingInfoAsync(Guid orderId, UpdateShippingDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException("Sipariş", orderId);

        order.ShippingCompany = dto.ShippingCompany;
        order.TrackingNumber = dto.TrackingNumber;

        if (!string.IsNullOrEmpty(dto.TrackingNumber) && order.Status == OrderStatus.Preparing)
        {
            order.Status = OrderStatus.Shipped;
            order.ShippedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrderStatusAdminAsync(Guid orderId, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException("Sipariş", orderId);

        var newStatus = Enum.Parse<OrderStatus>(dto.NewStatus);

        order.Status = newStatus;

        switch (newStatus)
        {
            case OrderStatus.Preparing:
                break;
            case OrderStatus.Shipped:
                order.ShippedAt = DateTime.UtcNow;
                order.ShippingCompany = dto.ShippingCompany;
                order.TrackingNumber = dto.TrackingNumber;
                break;
            case OrderStatus.Delivered:
                order.DeliveredAt = DateTime.UtcNow;
                break;
        }

        await _context.SaveChangesAsync();
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
        ReturnReason: o.ReturnReason,
        CancellationReason: o.CancellationReason,
        AdminReturnNote: o.AdminReturnNote,
        ReturnRequestedAt: o.ReturnRequestedAt,
        RefundedAt: o.RefundedAt,
        RefundAmount: o.RefundAmount,
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
