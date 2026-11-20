using System.Text.Json;
using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Application.Conversations.Waiter;
using MeuGarcom.Domain.Combos;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Conversations;
using MeuGarcom.Domain.Menus;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Conversations.Commands.SendMessage;

/// <summary>
/// Customer message → limits (domain) → input guardrail → AI Waiter with output guardrail
/// (one regeneration) → persisted reply. Fixed replies come from <see cref="FixedReplies"/>.
/// </summary>
public sealed class SendMessageHandler(
    IAppDbContext db,
    InputGuard inputGuard,
    WaiterTurn waiterTurn,
    TimeProvider clock) : ICommandHandler<SendMessageCommand, Result<WaiterReplyDto>>
{
    public const int HistoryLength = 12;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<Result<WaiterReplyDto>> Handle(SendMessageCommand command, CancellationToken ct)
    {
        var conversation = await db.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == command.ConversationId, ct);
        if (conversation is null)
            return ConversationErrors.NotFound;

        var now = clock.GetUtcNow();
        var received = conversation.ReceiveCustomerMessage(command.Content, now);
        if (received.IsFailure)
        {
            await db.SaveChangesAsync(ct); // persists the conversation closing on limits
            return received.Error!;
        }

        var unit = await db.Units.AsNoTracking()
            .Where(u => u.Id == conversation.UnitId)
            .Select(u => new
            {
                u.RestaurantId,
                u.ComboBudgetTolerance,
                RestaurantName = db.Restaurants.Where(r => r.Id == u.RestaurantId).Select(r => r.Name).First(),
            })
            .FirstAsync(ct);

        var verdict = await inputGuard.EvaluateAsync(received.Value.Content, unit.RestaurantId, ct);
        conversation.DetectLanguage(verdict.Language);

        if (verdict.Decision is InputDecision.Blocked)
        {
            Record(conversation, verdict.EventKind!.Value, now);
            return await ReplyFixedAsync(conversation, FixedReply.Blocked, ReplyOutcome.Blocked, now, ct);
        }

        if (verdict.Decision is InputDecision.Abusive)
        {
            Record(conversation, GuardrailEventKind.AbusiveMessage, now);
            var closed = conversation.RegisterAbuse();
            return await ReplyFixedAsync(
                conversation, closed ? FixedReply.AbuseClosed : FixedReply.Abuse, ReplyOutcome.Abuse, now, ct);
        }

        var tableNumber = conversation.DiningTableId is { } tableId
            ? await db.DiningTables.Where(t => t.Id == tableId).Select(t => (int?)t.Number).FirstOrDefaultAsync(ct)
            : null;
        var menu = await db.LoadMenuForUnitAsync(conversation.UnitId, ct);
        var snapshot = MenuSnapshot.Build(menu, unit.RestaurantName, tableNumber, unit.ComboBudgetTolerance);

        var history = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .TakeLast(HistoryLength)
            .Select(m => new AiMessage(m.Role == MessageRole.Customer ? AiRole.User : AiRole.Assistant, m.Content))
            .ToList();
        var customerTexts = conversation.Messages
            .Where(m => m.Role == MessageRole.Customer)
            .Select(m => m.Content)
            .ToList();

        var turn = await waiterTurn.RunAsync(
            new WaiterTurnInput(unit.RestaurantId, snapshot, conversation.ReplyLanguage, history, customerTexts), ct);

        for (var i = 0; i < turn.RejectedAttempts; i++)
            Record(conversation, GuardrailEventKind.OutputRejected, now);

        if (turn.Reply is null)
        {
            Record(conversation, turn.ProviderFailed ? GuardrailEventKind.ProviderFailure : GuardrailEventKind.SafeFallbackUsed, now);
            return await ReplyFixedAsync(conversation, FixedReply.SafeFallback, ReplyOutcome.SafeFallback, now, ct,
                turn.Provider, turn.Model);
        }

        var combo = turn.Reply.Combo is { } suggestion ? ToDto(suggestion, conversation.ReplyLanguage) : null;
        conversation.Reply(new AssistantReply(
            turn.Reply.Text,
            turn.Reply.ItemIds,
            combo is null ? null : JsonSerializer.Serialize(combo, JsonOptions),
            ReplyOutcome.Answered,
            turn.Provider,
            turn.Model), now);
        await db.SaveChangesAsync(ct);

        var items = turn.Reply.ItemIds
            .Select(id => snapshot.Items.First(i => i.Id == id))
            .Select(ToDto)
            .ToList();
        return new WaiterReplyDto(turn.Reply.Text, items, combo, "answered",
            conversation.Status == ConversationStatus.Closed, turn.Reply.QuickReplies);
    }

    private async Task<Result<WaiterReplyDto>> ReplyFixedAsync(
        Conversation conversation, FixedReply fixedReply, ReplyOutcome outcome, DateTimeOffset now,
        CancellationToken ct, string? provider = null, string? model = null)
    {
        var text = FixedReplies.Get(fixedReply, conversation.ReplyLanguage);
        conversation.Reply(new AssistantReply(text, [], null, outcome, provider, model), now);
        await db.SaveChangesAsync(ct);

        var outcomeName = outcome switch
        {
            ReplyOutcome.Blocked => "blocked",
            ReplyOutcome.Abuse => "abuse",
            _ => "safe_fallback",
        };
        return new WaiterReplyDto(text, [], null, outcomeName, conversation.Status == ConversationStatus.Closed, []);
    }

    private void Record(Conversation conversation, GuardrailEventKind kind, DateTimeOffset now) =>
        db.GuardrailEvents.Add(GuardrailEvent.Record(conversation.UnitId, conversation.Id, kind, now));

    private static ChatItemDto ToDto(MenuItem item)
    {
        var tags = new List<string>(4);
        if (item.IsVegan) tags.Add("vegan");
        if (item.IsLactoseFree) tags.Add("lactose_free");
        if (item.IsGlutenFree) tags.Add("gluten_free");
        if (item.IsSpicy) tags.Add("spicy");
        return new ChatItemDto(item.Id, item.Name, item.Price, item.Description, item.Portion, item.ServesPeople, tags);
    }

    private static ChatComboDto ToDto(ComboSuggestion combo, string language) => new(
        combo.Budget,
        combo.Total,
        combo.IsOverBudget,
        combo.IsOverBudget
            ? FixedReplies.Get(FixedReply.ComboOverBudget, language, FixedReplies.FormatBrl(combo.Budget), FixedReplies.FormatBrl(combo.Total))
            : null,
        combo.Lines.Select(l => new ChatComboLineDto(l.ItemId, l.ItemName, l.Quantity, l.UnitPrice, l.Subtotal)).ToList());
}
