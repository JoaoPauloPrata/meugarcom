using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.DeleteMenuItem;

public sealed class DeleteMenuItemHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IMenuImageStorage images,
    TimeProvider clock)
    : ICommandHandler<DeleteMenuItemCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteMenuItemCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var imagePath = menu.Items.FirstOrDefault(i => i.Id == command.ItemId)?.ImagePath;
        var removed = menu.RemoveItem(command.ItemId, clock.GetUtcNow());
        if (removed.IsFailure)
            return removed;

        await db.SaveChangesAsync(ct);
        if (imagePath is not null)
            await images.DeleteAsync([imagePath], CancellationToken.None);

        return Result.Success();
    }
}
