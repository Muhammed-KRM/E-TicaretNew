using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ETicaret.Business.Interfaces;
using ETicaret.Business.Services;

namespace ETicaret.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Core Services
        services.AddScoped<IAuthService, AuthManager>();
        services.AddScoped<IUserService, UserManager>();
        services.AddScoped<IAdminService, AdminManager>();
        services.AddScoped<ISettingService, SettingManager>();
        services.AddScoped<ILogService, LogManager>();
        services.AddScoped<IEmailService, ETicaret.Business.Infrastructure.Email.SmtpEmailService>();
        services.AddScoped<IModerationService, ModerationManager>();
        
        // E-Commerce Services
        services.AddScoped<IProductService, ProductManager>();
        services.AddScoped<ICategoryService, CategoryManager>();
        services.AddScoped<ICartService, CartManager>();
        services.AddScoped<IOrderService, OrderManager>();
        services.AddScoped<IAddressService, AddressManager>();
        services.AddScoped<ICouponService, CouponManager>();
        services.AddScoped<IReviewService, ReviewManager>();

        services.AddMemoryCache();

        // Notification Services
        services.AddScoped<INotificationService, NotificationManager>();
        services.AddScoped<ISmsService, ETicaret.Business.Infrastructure.Sms.NetgsmSmsService>();
        services.AddScoped<ETicaret.Business.Infrastructure.Messaging.IFcmService, ETicaret.Business.Infrastructure.Messaging.FcmService>();
        services.AddHttpClient("Netgsm");
        services.AddHttpClient<ETicaret.Business.Infrastructure.Messaging.FcmService>();

        // FluentValidation — Tüm Validator'ları kaydet
        services.AddValidatorsFromAssemblyContaining<AuthManager>();

        // Elasticsearch
        ETicaret.Business.Infrastructure.Search.ElasticsearchExtensions.AddElasticsearch(services);
        services.AddScoped<ISearchService, ETicaret.Business.Infrastructure.Search.ElasticsearchService>();

        // Redis
        services.AddSingleton<ICacheService, ETicaret.Business.Infrastructure.Cache.RedisCacheService>();

        // Payment
        services.AddScoped<IPaymentService, ETicaret.Business.Infrastructure.Payment.IyzicoPaymentService>();

        // File Storage
        services.AddScoped<IFileStorageService, ETicaret.Business.Infrastructure.Storage.LocalFileStorageService>();

        // MassTransit Dummy Publish Endpoint
        services.AddScoped<MassTransit.IPublishEndpoint, ETicaret.Business.Infrastructure.Messaging.DummyPublishEndpoint>();
        
        return services;
    }
}
