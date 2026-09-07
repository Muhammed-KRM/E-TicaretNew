using Microsoft.Extensions.DependencyInjection;
using ETicaret.Business.Interfaces;
using ETicaret.SharedUI.ApiServices;

namespace ETicaret.SharedUI;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedApiServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthApiService>();
        services.AddScoped<IProductService, ProductApiService>();
        services.AddScoped<ICartService, CartApiService>();
        services.AddScoped<IOrderService, OrderApiService>();
        services.AddScoped<IAddressService, AddressApiService>();
        services.AddScoped<IReviewService, ReviewApiService>();
        services.AddScoped<IUserService, UserApiService>();
        services.AddScoped<ICategoryService, CategoryApiService>();
        services.AddScoped<IContactService, ContactApiService>();
        services.AddScoped<IWishlistService, WishlistApiService>();
        
        // API Services without interfaces
        services.AddScoped<WishlistApiService>();
        
        return services;
    }
}

