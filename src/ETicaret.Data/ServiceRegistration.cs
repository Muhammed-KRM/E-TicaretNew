using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ETicaret.Data.Context;
using ETicaret.Data.Repositories;

namespace ETicaret.Data;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add Repositories
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}

