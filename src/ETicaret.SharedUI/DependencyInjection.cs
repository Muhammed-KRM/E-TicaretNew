using Microsoft.Extensions.DependencyInjection;
using ETicaret.Business.Interfaces;
using ETicaret.SharedUI.ApiServices;

namespace ETicaret.SharedUI;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedApiServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthApiService>();
        services.AddScoped<IListingService, ListingApiService>();
        services.AddScoped<ITokenService, TokenApiService>();
        services.AddScoped<IMessageService, MessageApiService>();
        services.AddScoped<IReviewService, ReviewApiService>();
        services.AddScoped<IVitrinService, VitrinApiService>();
        services.AddScoped<IUserService, UserApiService>();
        
        return services;
    }
}

