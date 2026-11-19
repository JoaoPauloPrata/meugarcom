using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Conversations;

public enum GuardrailEventKind
{
    InputPatternBlocked,
    InputClassifierBlocked,
    AbusiveMessage,
    OutputRejected,
    SafeFallbackUsed,
    ProviderFailure,
}

/// <summary>Monitoring record of a guardrail action. Stores no message content (no personal data).</summary>
public sealed class GuardrailEvent : Entity
{
    private GuardrailEvent() { }

    public Guid UnitId { get; private set; }
    public Guid? ConversationId { get; private set; }
    public GuardrailEventKind Kind { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static GuardrailEvent Record(Guid unitId, Guid? conversationId, GuardrailEventKind kind, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        UnitId = unitId,
        ConversationId = conversationId,
        Kind = kind,
        CreatedAt = now,
    };
}
