using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ETicaret.Business.Interfaces;
using ETicaret.Business.Services;

namespace ETicaret.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Manager'lar (Servisler)
        services.AddScoped<IAuthService, AuthManager>();
        services.AddScoped<IListingService, ListingManager>();
        services.AddScoped<ITokenService, TokenManager>();
        services.AddScoped<IMessageService, MessageManager>();
        services.AddScoped<IReviewService, ReviewManager>();
        services.AddScoped<IVitrinService, VitrinManager>();
        services.AddScoped<IUserService, UserManager>();
        services.AddScoped<IAdminService, AdminManager>();
        services.AddScoped<ISettingService, SettingManager>();
        services.AddScoped<ILogService, LogManager>();
        services.AddMemoryCache();
        services.AddScoped<IEmailService, ETicaret.Business.Infrastructure.Email.SmtpEmailService>();
        services.AddScoped<IModerationService, ModerationManager>();

        // Bildirim Sistemi
        services.AddScoped<INotificationService, NotificationManager>();
        services.AddScoped<ISmsService, ETicaret.Business.Infrastructure.Sms.NetgsmSmsService>();
        services.AddScoped<ETicaret.Business.Infrastructure.Messaging.IFcmService, ETicaret.Business.Infrastructure.Messaging.FcmService>();
        services.AddHttpClient("Netgsm");
        services.AddHttpClient<ETicaret.Business.Infrastructure.Messaging.FcmService>();

        // FluentValidation — Bu assembly'deki tüm Validator'ları otomatik tarayıp kaydet
        services.AddValidatorsFromAssemblyContaining<AuthManager>();

        // Adım 3.2: Elasticsearch
        ETicaret.Business.Infrastructure.Search.ElasticsearchExtensions.AddElasticsearch(services);
        services.AddScoped<ISearchService, ETicaret.Business.Infrastructure.Search.ElasticsearchService>();

        // Adım 3.3: Redis
        services.AddSingleton<ICacheService, ETicaret.Business.Infrastructure.Cache.RedisCacheService>();

        // Ödeme Sistemi — Strategy + Factory Pattern (PayTR yurt içi, Stripe yurt dışı)
        services.AddScoped<IPaymentService, ETicaret.Business.Infrastructure.Payment.PayTRPaymentService>();
        services.AddScoped<IPaymentService, ETicaret.Business.Infrastructure.Payment.StripePaymentService>();
        services.AddScoped<IPaymentServiceFactory, ETicaret.Business.Infrastructure.Payment.PaymentServiceFactory>();

        // Dosya Yükleme (Local → ileride Azure Blob'a geçilebilir)
        services.AddScoped<IFileStorageService, ETicaret.Business.Infrastructure.Storage.LocalFileStorageService>();

        // RabbitMQ/MassTransit — RabbitMQ disabled olduğunda DummyPublishEndpoint kullanılır
        // RabbitMQ enabled olduğunda Program.cs'deki AddMassTransit bu kaydın üzerine yazar
        services.AddScoped<MassTransit.IPublishEndpoint, ETicaret.Business.Infrastructure.Messaging.DummyPublishEndpoint>();
        
        return services;
    }
}

