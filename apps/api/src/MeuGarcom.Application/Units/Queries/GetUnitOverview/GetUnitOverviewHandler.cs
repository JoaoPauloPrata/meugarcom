using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Conversations;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Units.Queries.GetUnitOverview;

public sealed class GetUnitOverviewHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : IQueryHandler<GetUnitOverviewQuery, Result<UnitOverviewDto>>
{
    private static readonly TimeZoneInfo BrazilTime = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    private static readonly (string Attribute, string[] Terms)[] DietaryTerms =
    [
        ("lactose_free", ["lactose", "leite", "lacteo", "dairy", "lactosa"]),
        ("gluten_free", ["gluten", "glúten", "celiac", "celíac"]),
        ("vegan", ["vegan", "vegetarian"]),
    ];

    public async ValueTask<Result<UnitOverviewDto>> Handle(GetUnitOverviewQuery query, CancellationToken ct)
    {
        var owned = await db.UnitsOwnedBy(currentUser.UserId).AnyAsync(u => u.Id == query.UnitId, ct);
        if (!owned)
            return UnitErrors.NotFound;

        var now = clock.GetUtcNow();
        var localNow = TimeZoneInfo.ConvertTime(now, BrazilTime);
        var startOfToday = new DateTimeOffset(localNow.Date, localNow.Offset).ToUniversalTime();
        var last7Days = now.AddDays(-7);
        var last30Days = now.AddDays(-30);

        var conversationsToday = await db.Conversations
            .CountAsync(c => c.UnitId == query.UnitId && c.StartedAt >= startOfToday, ct);

        var customerMessages = db.ConversationMessages.Where(m =>
            m.Role == MessageRole.Customer &&
            db.Conversations.Any(c => c.Id == m.ConversationId && c.UnitId == query.UnitId));

        var questionsToday = await customerMessages.CountAsync(m => m.CreatedAt >= startOfToday, ct);

        var recentTimestamps = await customerMessages
            .Where(m => m.CreatedAt >= last7Days)
            .Select(m => m.CreatedAt)
            .ToListAsync(ct);
        var byHour = new int[24];
        foreach (var timestamp in recentTimestamps)
            byHour[TimeZoneInfo.ConvertTime(timestamp, BrazilTime).Hour]++;

        var recentTexts = await customerMessages
            .Where(m => m.CreatedAt >= last30Days)
            .Select(m => m.Content.ToLower())
            .ToListAsync(ct);
        var dietary = DietaryTerms
            .Select(d => new DietaryQuestionCount(d.Attribute, recentTexts.Count(t => d.Terms.Any(t.Contains))))
            .ToList();

        var menuItems = await db.MenuItems
            .CountAsync(i => db.Menus.Any(m => m.Id == i.MenuId && m.UnitId == query.UnitId), ct);
        var tables = await db.DiningTables.CountAsync(t => t.UnitId == query.UnitId, ct);
        var blocked = await db.GuardrailEvents.CountAsync(e =>
            e.UnitId == query.UnitId && e.CreatedAt >= last7Days &&
            (e.Kind == GuardrailEventKind.InputPatternBlocked || e.Kind == GuardrailEventKind.InputClassifierBlocked), ct);

        return new UnitOverviewDto(conversationsToday, questionsToday, menuItems, tables, blocked, byHour, dietary);
    }
}
