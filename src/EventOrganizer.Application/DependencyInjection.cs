using EventOrganizer.Application.Interfaces;
using EventOrganizer.Application.Services;
using EventOrganizer.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventOrganizer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string frontendUrl)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.AddScoped<IAuthService>(sp => new AuthService(
            sp.GetRequiredService<EventOrganizer.Domain.Interfaces.IUserRepository>(),
            sp.GetRequiredService<IPasswordHasher>(),
            sp.GetRequiredService<ITokenService>(),
            sp.GetRequiredService<IEmailService>(),
            frontendUrl));

        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
