using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.UpdateMenuItem;

public sealed class UpdateMenuItemHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<UpdateMenuItemCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateMenuItemCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var now = clock.GetUtcNow();
        var updated = menu.UpdateItem(command.ItemId, command.Item.CategoryId, command.Item.ToDetails(), now);
        if (updated.IsFailure)
            return updated;

        // Manual review by a person in the menu settings.
        var dietary = menu.SetItemDietaryAttributes(command.ItemId, command.Item.ToDietary(), now);
        if (dietary.IsFailure)
            return dietary;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
