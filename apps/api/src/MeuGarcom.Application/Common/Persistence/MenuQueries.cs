using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Menus;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Common.Persistence;

public static class MenuQueries
{
    /// <summary>Loads the full menu aggregate (categories + items) of a unit owned by the user.</summary>
    public static Task<Menu?> LoadOwnedMenuAsync(
        this IAppDbContext db, Guid userId, Guid unitId, CancellationToken ct, bool tracking = true)
    {
        var query = db.Menus
            .Include(m => m.Categories)
            .Include(m => m.Items)
            .Where(m => m.UnitId == unitId && db.UnitsOwnedBy(userId).Any(u => u.Id == unitId));

        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(ct);
    }

    /// <summary>Loads the full menu aggregate of a unit, without ownership check (public chat).</summary>
    public static Task<Menu?> LoadMenuForUnitAsync(this IAppDbContext db, Guid unitId, CancellationToken ct) =>
        db.Menus
            .AsNoTracking()
            .Include(m => m.Categories)
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.UnitId == unitId, ct);
}
