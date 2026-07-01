using FluentValidation;
using Tacdent.Api.Auth;
using Tacdent.Api.Factories;
using Tacdent.Api.Validators;
using Tacdent.Application.Services;
using Tacdent.Application.Services.Interfaces;

namespace Tacdent.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationLayer(this IServiceCollection services)
    {
        services.AddHttpClient<IRecaptchaValidator, RecaptchaValidator>();

        services.AddScoped<IAppointmentFactory, AppointmentFactory>();
        services.AddScoped<IServiceFactory, ServiceFactory>();
        services.AddScoped<IUserFactory, UserFactory>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddValidatorsFromAssemblyContaining<CreateAppointmentRequestValidator>();

        return services;
    }
}
