using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Conversations;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Conversations.Queries.ListConversations;

public sealed class ListConversationsHandler(IAppDbContext db, ICurrentUser currentUser)
    : IQueryHandler<ListConversationsQuery, Result<IReadOnlyList<ConversationSummaryDto>>>
{
    public async ValueTask<Result<IReadOnlyList<ConversationSummaryDto>>> Handle(ListConversationsQuery query, CancellationToken ct)
    {
        var owned = await db.UnitsOwnedBy(currentUser.UserId).AnyAsync(u => u.Id == query.UnitId, ct);
        if (!owned)
            return UnitErrors.NotFound;

        var conversations = await db.Conversations.AsNoTracking()
            .Include(c => c.Messages)
            .Where(c => c.UnitId == query.UnitId && c.CustomerMessageCount > 0)
            .OrderByDescending(c => c.StartedAt)
            .Take(Math.Clamp(query.Limit, 1, 100))
            .ToListAsync(ct);

        var tableIds = conversations.Where(c => c.DiningTableId.HasValue).Select(c => c.DiningTableId!.Value).Distinct().ToList();
        var tableNumbers = await db.DiningTables.AsNoTracking()
            .Where(t => tableIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Number, ct);

        return conversations.Select(c => new ConversationSummaryDto(
            c.Id,
            c.DiningTableId is { } id && tableNumbers.TryGetValue(id, out var number) ? number : null,
            c.Language,
            c.Status == ConversationStatus.Active ? "active" : "closed",
            c.StartedAt,
            c.LastMessageAt,
            c.Messages.OrderBy(m => m.CreatedAt).Select(m => new ConversationMessageDto(
                m.Role == MessageRole.Customer ? "customer" : "assistant",
                m.Content,
                m.Outcome?.ToString(),
                m.CreatedAt)).ToList())).ToList();
    }
}
