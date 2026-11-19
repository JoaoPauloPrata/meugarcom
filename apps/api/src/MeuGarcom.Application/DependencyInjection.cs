using FluentValidation;
using MeuGarcom.Application.Conversations.Waiter;
using Microsoft.Extensions.DependencyInjection;

namespace MeuGarcom.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<InputGuard>();
        services.AddScoped<WaiterTurn>();
        return services;
    }
}
