using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Tacdent.Api;
using Tacdent.Api.Auth;
using Tacdent.Api.Exceptions;
using Tacdent.Api.Extensions;
using Tacdent.Api.Filters;
using Tacdent.Api.Json;
using Tacdent.Api.Logging;
using Tacdent.Api.Middleware;
using Tacdent.Api.Options;
using Tacdent.Application;
using Tacdent.Application.Options;
using Tacdent.Data;
using Tacdent.Data.Context;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services
        .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
        });

    builder.Services.AddOpenApi();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.Configure<RequestLoggingOptions>(
        builder.Configuration.GetSection(RequestLoggingOptions.SectionName));
    builder.Services.AddSingleton<SensitiveDataRedactor>();

    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
        ?? throw new InvalidOperationException("Jwt configuration is missing.");

    var adminPassword = builder.Configuration.GetSection(AuthOptions.SectionName)["AdminPassword"] ?? string.Empty;

    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "Auth:AdminPassword is not configured. Set it via user secrets, environment variables, or appsettings.");
    }

    if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Key must be at least 32 characters. Set it via user secrets, environment variables, or appsettings.");
    }

    var recaptchaOptions = builder.Configuration.GetSection(RecaptchaOptions.SectionName).Get<RecaptchaOptions>()
        ?? new RecaptchaOptions();

    if (recaptchaOptions.Enabled && string.IsNullOrWhiteSpace(recaptchaOptions.SecretKey))
    {
        throw new InvalidOperationException(
            "Recaptcha:SecretKey is not configured while Recaptcha:Enabled is true. Set it via user secrets, environment variables, or appsettings, or set Recaptcha:Enabled to false.");
    }

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.Configure<RecaptchaOptions>(builder.Configuration.GetSection(RecaptchaOptions.SectionName));
    builder.Services.Configure<InternalApiOptions>(builder.Configuration.GetSection(InternalApiOptions.SectionName));

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>();
        if (knownProxies is { Length: > 0 })
        {
            foreach (var proxy in knownProxies)
            {
                if (IPAddress.TryParse(proxy, out var ip))
                {
                    options.KnownProxies.Add(ip);
                }
            }

            return;
        }

        options.KnownProxies.Add(IPAddress.Loopback);
        options.KnownProxies.Add(IPAddress.IPv6Loopback);
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("login", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.GetClientIpAddress() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(5),
                }));
        options.AddPolicy("booking", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.GetClientIpAddress() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(15),
                }));
    });

    builder.Services.AddDataLayer(builder.Configuration);
    builder.Services.AddApplicationLayer(builder.Configuration);
    builder.Services.AddPresentationLayer();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins(
                    builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ??
                    ["http://localhost:3000"])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders(TraceContext.CorrelationIdHeader);
        });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TacdentDbContext>();
        await dbContext.Database.MigrateAsync();

        await AdminSeeder.SeedAsync(scope.ServiceProvider);
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("TacDent API");
        });
    }
    else
    {
        app.UseHsts();
    }

    app.UseForwardedHeaders();
    app.UseMiddleware<CorrelationMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseExceptionHandler();

    app.Use(async (context, next) =>
    {
        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        await next();
    });

    app.UseMiddleware<InternalApiMiddleware>();
    app.UseCors("Frontend");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();

    Log.Information("Starting TacDent API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
