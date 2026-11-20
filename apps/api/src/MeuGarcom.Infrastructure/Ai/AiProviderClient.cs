using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Ai;

namespace MeuGarcom.Infrastructure.Ai;

/// <summary>Per-call options derived from the task and from <c>AiModelSettings</c>.</summary>
internal sealed record ModelCallOptions(int MaxOutputTokens, decimal? Temperature);

/// <summary>One AI provider (OpenAI, Anthropic) able to return JSON constrained to a schema.</summary>
internal interface IAiProviderClient
{
    AiProvider Provider { get; }

    /// <summary>False when the API key is not configured; the orchestrator skips the provider.</summary>
    bool IsConfigured { get; }

    Task<string> CompleteAsync(AiModelTarget target, ModelCallOptions options, AiCompletionRequest request, CancellationToken ct);
}

internal sealed class AiProviderException(string message) : Exception(message);
