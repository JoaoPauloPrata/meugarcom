using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Conversations;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Conversations.Queries.ListConversations;

/// <summary>Most recent conversations of a unit, with their messages (back office).</summary>
public sealed record ListConversationsQuery(Guid UnitId, int Limit = 30) : IQuery<Result<IReadOnlyList<ConversationSummaryDto>>>;

public sealed record ConversationSummaryDto(
    Guid Id,
    int? TableNumber,
    string? Language,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? LastMessageAt,
    IReadOnlyList<ConversationMessageDto> Messages);

public sealed record ConversationMessageDto(string Role, string Content, string? Outcome, DateTimeOffset CreatedAt);
