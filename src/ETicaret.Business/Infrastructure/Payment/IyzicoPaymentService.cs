using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using ETicaret.Business.Interfaces;

namespace ETicaret.Business.Infrastructure.Payment;

public class IyzicoPaymentService : IPaymentService
{
    public string ProviderName => "Iyzico";

    private readonly Iyzipay.Options _options;
    private readonly ILogService _logService;

    public IyzicoPaymentService(IConfiguration configuration, ILogService logService)
    {
        _logService = logService;
        _options = new Iyzipay.Options
        {
            ApiKey = configuration["Iyzico:ApiKey"],
            SecretKey = configuration["Iyzico:SecretKey"],
            BaseUrl = configuration["Iyzico:BaseUrl"] ?? "https://sandbox-api.iyzipay.com"
        };
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var iyziRequest = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = request.OrderId.ToString(),
                Price = request.Amount.ToString("F2").Replace(",", "."),
                PaidPrice = request.Amount.ToString("F2").Replace(",", "."),
                Currency = Currency.TRY.ToString(),
                BasketId = request.OrderId.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = request.ReturnUrl,
                EnabledInstallments = new List<int> { 2, 3, 6, 9 }
            };

            var buyer = new Buyer
            {
                Id = request.UserId.ToString(),
                Name = request.BuyerName?.Split(' ').FirstOrDefault() ?? "Müşteri",
                Surname = request.BuyerName?.Split(' ').LastOrDefault() ?? "Soyadı",
                GsmNumber = "+905555555555", // Test numarası
                Email = request.BuyerEmail ?? "email@email.com",
                IdentityNumber = "74300864791",
                LastLoginDate = "2015-10-05 12:43:35",
                RegistrationDate = "2013-04-21 15:12:09",
                RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                Ip = request.BuyerIp ?? "85.34.78.112",
                City = "Istanbul",
                Country = "Turkey",
                ZipCode = "34732"
            };
            iyziRequest.Buyer = buyer;

            var shippingAddress = new Address
            {
                ContactName = request.BuyerName ?? "Müşteri",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                ZipCode = "34742"
            };
            iyziRequest.ShippingAddress = shippingAddress;

            var billingAddress = new Address
            {
                ContactName = request.BuyerName ?? "Müşteri",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                ZipCode = "34742"
            };
            iyziRequest.BillingAddress = billingAddress;

            var basketItems = new List<BasketItem>();
            
            if(request.BasketItems != null && request.BasketItems.Any())
            {
                foreach (var item in request.BasketItems)
                {
                    basketItems.Add(new BasketItem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Category1 = item.Category,
                        ItemType = BasketItemType.PHYSICAL.ToString(),
                        Price = item.Price.ToString("F2").Replace(",", ".")
                    });
                }
            }
            else
            {
                basketItems.Add(new BasketItem
                {
                    Id = request.OrderId.ToString(),
                    Name = request.Description,
                    Category1 = "Diğer",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = request.Amount.ToString("F2").Replace(",", ".")
                });
            }

            iyziRequest.BasketItems = basketItems;

            // Iyzico SDK calls
            CheckoutFormInitialize checkoutFormInitialize = await CheckoutFormInitialize.Create(iyziRequest, _options);

            if (checkoutFormInitialize.Status == "success")
            {
                return new PaymentResult
                {
                    Success = true,
                    RedirectUrl = checkoutFormInitialize.PaymentPageUrl,
                    TransactionId = checkoutFormInitialize.Token
                };
            }

            return new PaymentResult
            {
                Success = false,
                ErrorMessage = checkoutFormInitialize.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync("PAY-001", ex, request);
            return new PaymentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> VerifyCallbackAsync(Dictionary<string, string> callbackData)
    {
        try
        {
            if (!callbackData.TryGetValue("token", out string? token))
                return false;

            var request = new RetrieveCheckoutFormRequest
            {
                Token = token
            };

            CheckoutForm checkoutForm = await CheckoutForm.Retrieve(request, _options);

            return checkoutForm.Status == "success" && checkoutForm.PaymentStatus == "SUCCESS";
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync("PAY-002", ex, callbackData);
            return false;
        }
    }

    public async Task<bool> RefundAsync(string transactionId, decimal amount)
    {
        try
        {
            var request = new CreateRefundRequest
            {
                PaymentTransactionId = transactionId,
                Price = amount.ToString("F2").Replace(",", "."),
                Currency = Currency.TRY.ToString(),
                Ip = "85.34.78.112"
            };

            Refund refund = await Refund.Create(request, _options);
            return refund.Status == "success";
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync("PAY-003", ex, transactionId);
            return false;
        }
    }
}
