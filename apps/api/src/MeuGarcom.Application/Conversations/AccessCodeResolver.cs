using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Conversations;

public sealed record ResolvedAccess(
    Guid UnitId,
    Guid RestaurantId,
    string RestaurantName,
    string UnitName,
    Guid? TableId,
    int? TableNumber);

/// <summary>Resolves the public code of a QR code / NFC link: a table code or a unit-wide code.</summary>
public static class AccessCodeResolver
{
    public static async Task<ResolvedAccess?> ResolveAsync(this IAppDbContext db, string code, CancellationToken ct)
    {
        var normalized = code.Trim().ToLowerInvariant();
        if (!PublicCode.IsWellFormed(normalized))
            return null;

        var table = await db.DiningTables.AsNoTracking()
            .Where(t => t.PublicCode == normalized)
            .Select(t => new { t.Id, t.Number, t.UnitId })
            .FirstOrDefaultAsync(ct);

        var unitQuery = table is not null
            ? db.Units.Where(u => u.Id == table.UnitId)
            : db.Units.Where(u => u.PublicCode == normalized);

        var unit = await unitQuery.AsNoTracking()
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.RestaurantId,
                RestaurantName = db.Restaurants.Where(r => r.Id == u.RestaurantId).Select(r => r.Name).First(),
            })
            .FirstOrDefaultAsync(ct);

        return unit is null
            ? null
            : new ResolvedAccess(unit.Id, unit.RestaurantId, unit.RestaurantName, unit.Name, table?.Id, table?.Number);
    }
}
