using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Conversations;

public enum MessageRole
{
    Customer,
    Assistant,
}

public enum ReplyOutcome
{
    /// <summary>Answer produced by the model and approved by the output guardrail.</summary>
    Answered,

    /// <summary>Fixed reply: the input guardrail blocked the message (attack / injection).</summary>
    Blocked,

    /// <summary>Fixed neutral reply to an offensive message.</summary>
    Abuse,

    /// <summary>Fixed safe reply after the model output failed validation twice (or the providers failed).</summary>
    SafeFallback,
}

/// <summary>Assistant answer to persist. <paramref name="ComboJson"/> is the backend-computed combo, if any.</summary>
public sealed record AssistantReply(
    string Content,
    IReadOnlyList<Guid> ReferencedItemIds,
    string? ComboJson,
    ReplyOutcome Outcome,
    string? Provider,
    string? Model);

public sealed class ConversationMessage : Entity
{
    public const int MaxContentLength = 4000;

    private ConversationMessage() { }

    public Guid ConversationId { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = null!;
    public List<Guid> ReferencedItemIds { get; private set; } = [];
    public string? ComboJson { get; private set; }
    public ReplyOutcome? Outcome { get; private set; }
    public string? Provider { get; private set; }
    public string? Model { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    internal static ConversationMessage FromCustomer(Guid conversationId, string content, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        ConversationId = conversationId,
        Role = MessageRole.Customer,
        Content = content,
        CreatedAt = now,
    };

    internal static ConversationMessage FromAssistant(Guid conversationId, AssistantReply reply, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        ConversationId = conversationId,
        Role = MessageRole.Assistant,
        Content = reply.Content.Length > MaxContentLength ? reply.Content[..MaxContentLength] : reply.Content,
        ReferencedItemIds = reply.ReferencedItemIds.Distinct().ToList(),
        ComboJson = reply.ComboJson,
        Outcome = reply.Outcome,
        Provider = reply.Provider,
        Model = reply.Model,
        CreatedAt = now,
    };
}
