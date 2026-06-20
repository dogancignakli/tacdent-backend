using FluentValidation;
using Tacdent.Api.Auth;
using Tacdent.Api.Factories;
using Tacdent.Api.Validators;

namespace Tacdent.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationLayer(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentFactory, AppointmentFactory>();
        services.AddScoped<IServiceFactory, ServiceFactory>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddValidatorsFromAssemblyContaining<CreateAppointmentRequestValidator>();

        return services;
    }
}
