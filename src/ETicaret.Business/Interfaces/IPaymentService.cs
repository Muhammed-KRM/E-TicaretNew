namespace ETicaret.Business.Interfaces;

public interface IPaymentService
{
    string ProviderName { get; }
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<bool> VerifyCallbackAsync(Dictionary<string, string> callbackData);
    Task<bool> RefundAsync(string transactionId, decimal amount);
}

public class PaymentRequest
{
    public Guid? UserId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Description { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerIp { get; set; }
    
    // Sepet içeriklerini de alabiliriz iyzipay için
    public List<PaymentBasketItem> BasketItems { get; set; } = new();
}

public class PaymentBasketItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
