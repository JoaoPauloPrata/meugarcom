namespace MeuGarcom.Application.Common.Interfaces;

/// <summary>What the model is being used for; the orchestrator picks the model per task.</summary>
public enum AiTask
{
    /// <summary>Main AI Waiter answer (primary model, fallback on failure).</summary>
    WaiterReply,

    /// <summary>Input guardrail classifier (small and fast model).</summary>
    InputClassification,

    /// <summary>Menu extraction from an uploaded file (primary model, fallback on failure).</summary>
    MenuExtraction,
}

public enum AiRole
{
    User,
    Assistant,
}

public sealed record AiMessage(AiRole Role, string Content);

public sealed record AiAttachment(string FileName, string MediaType, byte[] Content);

/// <summary>
/// Provider-neutral request. Instructions and data travel in separate channels: the instructions
/// are the system prompt; <see cref="ContextData"/> is delimited data (never instructions); the
/// customer text always goes in <see cref="Messages"/> with the user role.
/// </summary>
public sealed record AiCompletionRequest(
    AiTask Task,
    Guid? RestaurantId,
    string Instructions,
    string? ContextData,
    IReadOnlyList<AiMessage> Messages,
    string SchemaName,
    string JsonSchema,
    AiAttachment? Attachment = null);

/// <summary>Raw JSON produced by the model (already constrained to the schema) and who produced it.</summary>
public sealed record AiCompletion(string Json, string Provider, string Model);

/// <summary>Thrown when every configured provider failed for a request.</summary>
public sealed class AiUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// Single entry point to the language models. Implementations live in Infrastructure: one per
/// provider (OpenAI, Anthropic) plus an orchestrator that reads <c>AiModelSettings</c> from the
/// database, chooses provider/model and falls back on errors, timeouts and rate limits.
/// </summary>
public interface IWaiterAssistant
{
    Task<AiCompletion> CompleteAsync(AiCompletionRequest request, CancellationToken cancellationToken);
}
