using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Accounts;

namespace MeuGarcom.Application.Common.Persistence;

public static class OwnershipQueries
{
    /// <summary>Units that belong to the account owned by <paramref name="userId"/>.</summary>
    public static IQueryable<Unit> UnitsOwnedBy(this IAppDbContext db, Guid userId) =>
        db.Units.Where(u => db.Restaurants.Any(r =>
            r.Id == u.RestaurantId && db.Accounts.Any(a => a.Id == r.AccountId && a.OwnerUserId == userId)));
}
