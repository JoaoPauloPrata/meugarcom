using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Ai;
using Microsoft.Extensions.Logging;

namespace MeuGarcom.Infrastructure.Ai;

/// <summary>
/// Orchestrator behind <see cref="IWaiterAssistant"/>: reads the provider/model from the database,
/// calls the primary provider and, on any error, timeout or rate limit, the fallback provider.
/// </summary>
internal sealed class FallbackWaiterAssistant(
    AiSettingsProvider settingsProvider,
    IEnumerable<IAiProviderClient> providers,
    ILogger<FallbackWaiterAssistant> logger) : IWaiterAssistant
{
    private const int ClassifierMaxTokens = 120;
    private const int ExtractionMaxTokens = 16_000;

    public async Task<AiCompletion> CompleteAsync(AiCompletionRequest request, CancellationToken cancellationToken)
    {
        var settings = await settingsProvider.GetAsync(request.RestaurantId, cancellationToken);

        AiModelTarget?[] candidates = request.Task == AiTask.InputClassification
            ? [settings.Classifier]
            : [settings.Primary, settings.Fallback];

        var options = request.Task switch
        {
            AiTask.InputClassification => new ModelCallOptions(ClassifierMaxTokens, 0m),
            AiTask.MenuExtraction => new ModelCallOptions(ExtractionMaxTokens, 0m),
            _ => new ModelCallOptions(settings.MaxOutputTokens, settings.Temperature),
        };
        var timeout = request.Task switch
        {
            AiTask.InputClassification => TimeSpan.FromSeconds(8),
            AiTask.MenuExtraction => TimeSpan.FromMinutes(3),
            _ => TimeSpan.FromSeconds(30),
        };

        Exception? lastError = null;
        foreach (var target in candidates.OfType<AiModelTarget>())
        {
            var provider = providers.FirstOrDefault(p => p.Provider == target.Provider);
            if (provider is not { IsConfigured: true })
            {
                logger.LogWarning("AI provider {Provider} is not configured; skipping {Model}", target.Provider, target.Model);
                continue;
            }

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);
            try
            {
                var json = await provider.CompleteAsync(target, options, request, timeoutSource.Token);
                return new AiCompletion(json, ProviderName(target.Provider), target.Model);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                lastError = ex;
                logger.LogWarning(ex, "AI provider {Provider}/{Model} failed for {Task}; trying the next one",
                    target.Provider, target.Model, request.Task);
            }
        }

        throw new AiUnavailableException($"No AI provider could complete {request.Task}.", lastError);
    }

    private static string ProviderName(AiProvider provider) => provider == AiProvider.OpenAi ? "openai" : "anthropic";
}
