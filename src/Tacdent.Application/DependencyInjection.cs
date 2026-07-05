using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tacdent.Application.Mapping;
using Tacdent.Application.Options;
using Tacdent.Application.Services;
using Tacdent.Application.Services.Interfaces;

namespace Tacdent.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.Configure<RecaptchaOptions>(configuration.GetSection(RecaptchaOptions.SectionName));

        // Mapperly mappers are stateless generated classes.
        services.AddSingleton<AppointmentMapper>();
        services.AddSingleton<ServiceMapper>();
        services.AddSingleton<TestimonialMapper>();
        services.AddSingleton<UserMapper>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<ITestimonialService, TestimonialService>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        return services;
    }
}
