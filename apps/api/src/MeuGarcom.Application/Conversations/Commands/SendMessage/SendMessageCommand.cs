using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Conversations.Commands.SendMessage;

public sealed record SendMessageCommand(Guid ConversationId, string Content) : ICommand<Result<WaiterReplyDto>>;

/// <param name="Outcome">answered | blocked | abuse | safe_fallback.</param>
/// <param name="ConversationClosed">True when the conversation was ended (abuse or limits).</param>
/// <param name="QuickReplies">Suggested next messages; a tap sends the text as the customer.</param>
public sealed record WaiterReplyDto(
    string Reply,
    IReadOnlyList<ChatItemDto> Items,
    ChatComboDto? Combo,
    string Outcome,
    bool ConversationClosed,
    IReadOnlyList<string> QuickReplies);

/// <param name="Tags">vegan | lactose_free | gluten_free | spicy (only the confirmed ones).</param>
public sealed record ChatItemDto(
    Guid Id,
    string Name,
    decimal Price,
    string? Description,
    string? Portion,
    int? ServesPeople,
    IReadOnlyList<string> Tags);

/// <param name="Total">Computed by the backend from stored prices, never by the model.</param>
/// <param name="Notice">Explicit warning when the total is above the budget.</param>
public sealed record ChatComboDto(
    decimal Budget,
    decimal Total,
    bool IsOverBudget,
    string? Notice,
    IReadOnlyList<ChatComboLineDto> Lines);

public sealed record ChatComboLineDto(Guid ItemId, string Name, int Quantity, decimal UnitPrice, decimal Subtotal);
