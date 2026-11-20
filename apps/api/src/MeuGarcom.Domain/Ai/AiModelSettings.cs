using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Ai;

public enum AiProvider
{
    OpenAi,
    Anthropic,
}

public sealed record AiModelTarget(AiProvider Provider, string Model, string? Effort);

/// <summary>
/// Default AI provider and model, read from the database (RestaurantId null = global default,
/// otherwise a per-restaurant override). Changing the model never requires a deploy.
/// </summary>
public sealed class AiModelSettings : Entity
{
    private AiModelSettings() { }

    public Guid? RestaurantId { get; private set; }
    public AiProvider PrimaryProvider { get; private set; }
    public string PrimaryModel { get; private set; } = null!;
    public string? PrimaryEffort { get; private set; }
    public AiProvider? FallbackProvider { get; private set; }
    public string? FallbackModel { get; private set; }
    public string? FallbackEffort { get; private set; }
    public AiProvider? ClassifierProvider { get; private set; }
    public string? ClassifierModel { get; private set; }
    public decimal? Temperature { get; private set; }
    public int MaxOutputTokens { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public AiModelTarget Primary => new(PrimaryProvider, PrimaryModel, PrimaryEffort);

    public AiModelTarget? Fallback =>
        FallbackProvider is { } provider && !string.IsNullOrWhiteSpace(FallbackModel)
            ? new AiModelTarget(provider, FallbackModel, FallbackEffort)
            : null;

    /// <summary>Small and fast model used by the input guardrail. Null disables the classifier.</summary>
    public AiModelTarget? Classifier =>
        ClassifierProvider is { } provider && !string.IsNullOrWhiteSpace(ClassifierModel)
            ? new AiModelTarget(provider, ClassifierModel, "none")
            : null;
}
