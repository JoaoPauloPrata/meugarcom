using FluentValidation;
using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Application.Common.Persistence;
using Unit = MeuGarcom.Domain.Accounts.Unit;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Tables.Commands.AddTables;

public sealed class AddTablesHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<AddTablesCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(AddTablesCommand command, CancellationToken ct)
    {
        var unit = await db.UnitsOwnedBy(currentUser.UserId)
            .Include(u => u.Tables)
            .FirstOrDefaultAsync(u => u.Id == command.UnitId, ct);
        if (unit is null)
            return UnitErrors.NotFound;

        var added = unit.AddTables(command.Count, clock.GetUtcNow());
        if (added.IsFailure)
            return added.Error!;

        await db.SaveChangesAsync(ct);
        return added.Value.Count;
    }
}
