using Microsoft.Extensions.DependencyInjection;
using UsersService.Application.Abstractions.Services;
using UsersService.Application.Services;

namespace UsersService.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Extension для добавления в DI бизнес логики
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
