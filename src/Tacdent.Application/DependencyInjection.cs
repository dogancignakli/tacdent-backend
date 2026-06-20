using Microsoft.Extensions.DependencyInjection;
using Tacdent.Application.Mapping;
using Tacdent.Application.Services;
using Tacdent.Application.Services.Interfaces;

namespace Tacdent.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // Mapperly mappers are stateless generated classes.
        services.AddSingleton<AppointmentMapper>();
        services.AddSingleton<ServiceMapper>();

        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();

        return services;
    }
}
