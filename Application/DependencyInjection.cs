using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Services;

namespace EventManagmentApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
