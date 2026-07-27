using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Keystone.Application;

public static class Installer
{
    public static IServiceCollection AddKeystoneApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>(sp => new Sender(sp));
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        return services;
    }

    public static IServiceCollection RegisterOperationHandlers(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IOperationHandler<>)), publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IOperationHandler<,>)), publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }

    public static IServiceCollection RegisterDomainEventHandlers(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }
}
