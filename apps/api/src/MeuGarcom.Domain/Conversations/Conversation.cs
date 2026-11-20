using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Conversations;

public enum ConversationStatus
{
    Active,
    Closed,
}

/// <summary>
/// Chat session of a customer at a table. A new one starts every time the chat is opened.
/// Enforces the usage limits (OWASP LLM10): message length, messages per conversation and
/// session duration.
/// </summary>
public sealed class Conversation : Entity
{
    public const int MaxCustomerMessageLength = 500;
    public const int MaxCustomerMessages = 40;
    public const int AbuseLimit = 3;
    public const string DefaultLanguage = "pt-BR";
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);

    private readonly List<ConversationMessage> _messages = [];

    private Conversation() { }

    public Guid UnitId { get; private set; }
    public Guid? DiningTableId { get; private set; }

    /// <summary>Language of the first customer message; used for the fixed replies.</summary>
    public string? Language { get; private set; }

    public ConversationStatus Status { get; private set; }
    public int CustomerMessageCount { get; private set; }
    public int AbuseCount { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }
    public IReadOnlyList<ConversationMessage> Messages => _messages.AsReadOnly();

    public string ReplyLanguage => Language ?? DefaultLanguage;

    public static Conversation Start(Guid unitId, Guid? diningTableId, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        UnitId = unitId,
        DiningTableId = diningTableId,
        Status = ConversationStatus.Active,
        StartedAt = now,
    };

    public Result<ConversationMessage> ReceiveCustomerMessage(string content, DateTimeOffset now)
    {
        if (Status == ConversationStatus.Closed)
            return ConversationErrors.Closed;
        if (now - StartedAt > MaxDuration)
        {
            Close();
            return ConversationErrors.Expired;
        }
        if (CustomerMessageCount >= MaxCustomerMessages)
        {
            Close();
            return ConversationErrors.MessageLimitReached;
        }

        var text = content?.Trim() ?? "";
        if (text.Length == 0)
            return ConversationErrors.MessageEmpty;
        if (text.Length > MaxCustomerMessageLength)
            return ConversationErrors.MessageTooLong;

        var message = ConversationMessage.FromCustomer(Id, text, now);
        _messages.Add(message);
        CustomerMessageCount++;
        LastMessageAt = now;
        return message;
    }

    /// <summary>Stores the language detected from the first customer message. Later changes are ignored.</summary>
    public void DetectLanguage(string? language)
    {
        if (Language is not null)
            return;

        var value = language?.Trim();
        Language = string.IsNullOrEmpty(value) || value.Length > 16 ? DefaultLanguage : value;
    }

    /// <summary>Registers an offensive message. Returns true when the conversation was closed.</summary>
    public bool RegisterAbuse()
    {
        AbuseCount++;
        if (AbuseCount < AbuseLimit)
            return false;

        Close();
        return true;
    }

    public ConversationMessage Reply(AssistantReply reply, DateTimeOffset now)
    {
        var message = ConversationMessage.FromAssistant(Id, reply, now);
        _messages.Add(message);
        LastMessageAt = now;
        return message;
    }

    public void Close() => Status = ConversationStatus.Closed;
}

public static class ConversationErrors
{
    public static readonly Error NotFound = Error.NotFound("conversation.not_found");
    public static readonly Error AccessCodeInvalid = Error.NotFound("conversation.access_code_invalid");
    public static readonly Error Closed = Error.Conflict("conversation.closed");
    public static readonly Error Expired = Error.Conflict("conversation.expired");
    public static readonly Error MessageLimitReached = Error.Conflict("conversation.message_limit_reached");
    public static readonly Error MessageEmpty = Error.Validation("conversation.message_empty");
    public static readonly Error MessageTooLong = Error.Validation("conversation.message_too_long");
}
