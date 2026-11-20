using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.SetMenuItemAvailability;

public sealed class SetMenuItemAvailabilityHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<SetMenuItemAvailabilityCommand, Result>
{
    public async ValueTask<Result> Handle(SetMenuItemAvailabilityCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var result = menu.SetItemAvailability(command.ItemId, command.IsAvailable, clock.GetUtcNow());
        if (result.IsFailure)
            return result;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
