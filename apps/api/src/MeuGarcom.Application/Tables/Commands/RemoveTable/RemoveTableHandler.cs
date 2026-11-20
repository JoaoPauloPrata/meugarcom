using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Tables.Commands.RemoveTable;

public sealed class RemoveTableHandler(IAppDbContext db, ICurrentUser currentUser)
    : ICommandHandler<RemoveTableCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveTableCommand command, CancellationToken ct)
    {
        var unit = await db.UnitsOwnedBy(currentUser.UserId)
            .Include(u => u.Tables)
            .FirstOrDefaultAsync(u => u.Id == command.UnitId, ct);
        if (unit is null)
            return UnitErrors.NotFound;

        var removed = unit.RemoveTable(command.TableId);
        if (removed.IsFailure)
            return removed;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
