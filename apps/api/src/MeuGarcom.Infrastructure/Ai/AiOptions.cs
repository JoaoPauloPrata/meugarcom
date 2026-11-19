namespace MeuGarcom.Infrastructure.Ai;

/// <summary>API keys come from environment variables / secrets, never from the database or the repo.</summary>
internal sealed record AiOptions(string? OpenAiApiKey, string? AnthropicApiKey);
