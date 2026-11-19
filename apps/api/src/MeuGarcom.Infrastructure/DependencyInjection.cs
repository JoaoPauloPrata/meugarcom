using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Infrastructure.Ai;
using MeuGarcom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeuGarcom.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddMemoryCache();
        services.AddSingleton(new AiOptions(config["OPENAI_API_KEY"], config["ANTHROPIC_API_KEY"]));
        services.AddHttpClient<OpenAiWaiterAssistant>(http =>
        {
            http.BaseAddress = new Uri("https://api.openai.com/");
            http.Timeout = TimeSpan.FromMinutes(4);
        });
        services.AddScoped<IAiProviderClient>(sp => sp.GetRequiredService<OpenAiWaiterAssistant>());
        services.AddSingleton<AnthropicWaiterAssistant>();
        services.AddScoped<IAiProviderClient>(sp => sp.GetRequiredService<AnthropicWaiterAssistant>());
        services.AddScoped<AiSettingsProvider>();
        services.AddScoped<IWaiterAssistant, FallbackWaiterAssistant>();

        return services;
    }
}
