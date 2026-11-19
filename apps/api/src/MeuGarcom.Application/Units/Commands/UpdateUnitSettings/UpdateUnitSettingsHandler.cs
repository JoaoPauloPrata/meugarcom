using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Units.Commands.UpdateUnitSettings;

public sealed class UpdateUnitSettingsHandler(IAppDbContext db, ICurrentUser currentUser)
    : ICommandHandler<UpdateUnitSettingsCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateUnitSettingsCommand command, CancellationToken ct)
    {
        var unit = await db.UnitsOwnedBy(currentUser.UserId).FirstOrDefaultAsync(u => u.Id == command.UnitId, ct);
        if (unit is null)
            return UnitErrors.NotFound;

        var restaurant = await db.Restaurants.FirstAsync(r => r.Id == unit.RestaurantId, ct);

        var renamed = restaurant.Rename(command.RestaurantName, command.CuisineType);
        if (renamed.IsFailure)
            return renamed;

        var details = unit.UpdateDetails(command.UnitName, command.City);
        if (details.IsFailure)
            return details;

        var tolerance = unit.ChangeComboBudgetTolerance(command.ComboBudgetTolerancePercent / 100m);
        if (tolerance.IsFailure)
            return tolerance;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
