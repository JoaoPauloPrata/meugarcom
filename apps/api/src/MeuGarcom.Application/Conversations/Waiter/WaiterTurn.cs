using MeuGarcom.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeuGarcom.Application.Conversations.Waiter;

public sealed record WaiterTurnInput(
    Guid RestaurantId,
    MenuSnapshot Menu,
    string ReplyLanguage,
    IReadOnlyList<AiMessage> History,
    IReadOnlyList<string> CustomerTexts);

/// <param name="Reply">Approved answer, or null when the safe fixed reply must be used.</param>
/// <param name="RejectedAttempts">How many model answers the output guardrail rejected.</param>
/// <param name="ProviderFailed">True when no provider could answer.</param>
public sealed record WaiterTurnResult(
    ValidatedReply? Reply,
    string? Provider,
    string? Model,
    int RejectedAttempts,
    bool ProviderFailed);

/// <summary>
/// One AI Waiter turn: model → output guardrail → (rejected) regenerate once with the problems
/// → (rejected again) safe reply. Sums, totals and validation are always done in code.
/// </summary>
public sealed class WaiterTurn(IWaiterAssistant assistant, ILogger<WaiterTurn> logger)
{
    public const int MaxAttempts = 2;

    public async Task<WaiterTurnResult> RunAsync(WaiterTurnInput input, CancellationToken ct)
    {
        var contextData = WaiterPrompt.ContextData(input.Menu, input.ReplyLanguage);
        var instructions = WaiterPrompt.Instructions;
        var rejected = 0;
        string? provider = null;
        string? model = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            AiCompletion completion;
            try
            {
                completion = await assistant.CompleteAsync(new AiCompletionRequest(
                    AiTask.WaiterReply,
                    input.RestaurantId,
                    instructions,
                    contextData,
                    input.History,
                    WaiterPrompt.SchemaName,
                    WaiterPrompt.Schema), ct);
            }
            catch (AiUnavailableException ex)
            {
                logger.LogError(ex, "No AI provider could answer the waiter turn");
                return new WaiterTurnResult(null, provider, model, rejected, ProviderFailed: true);
            }

            provider = completion.Provider;
            model = completion.Model;

            var check = OutputGuard.Check(WaiterOutput.TryParse(completion.Json), input.Menu, input.CustomerTexts);
            if (check.IsApproved)
                return new WaiterTurnResult(check.Reply, provider, model, rejected, ProviderFailed: false);

            rejected++;
            logger.LogInformation(
                "Output guardrail rejected attempt {Attempt} ({Provider}/{Model}): {Problems}",
                attempt, provider, model, string.Join(" | ", check.Problems));
            instructions = WaiterPrompt.WithCorrection(check.Problems);
        }

        return new WaiterTurnResult(null, provider, model, rejected, ProviderFailed: false);
    }
}
