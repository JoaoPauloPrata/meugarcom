using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.DeleteMenuItem;

public sealed class DeleteMenuItemHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<DeleteMenuItemCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteMenuItemCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var removed = menu.RemoveItem(command.ItemId, clock.GetUtcNow());
        if (removed.IsFailure)
            return removed;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
