using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Tables.Queries.ListTables;

public sealed class ListTablesHandler(IAppDbContext db, ICurrentUser currentUser)
    : IQueryHandler<ListTablesQuery, Result<TablesDto>>
{
    public async ValueTask<Result<TablesDto>> Handle(ListTablesQuery query, CancellationToken ct)
    {
        var unit = await db.UnitsOwnedBy(currentUser.UserId)
            .AsNoTracking()
            .Include(u => u.Tables)
            .FirstOrDefaultAsync(u => u.Id == query.UnitId, ct);
        if (unit is null)
            return UnitErrors.NotFound;

        return new TablesDto(
            unit.PublicCode,
            unit.Tables.OrderBy(t => t.Number).Select(t => new TableDto(t.Id, t.Number, t.PublicCode)).ToList());
    }
}
