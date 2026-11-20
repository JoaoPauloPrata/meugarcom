using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.RemoveMenuItemImage;

public sealed class RemoveMenuItemImageHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IMenuImageStorage storage,
    TimeProvider clock) : ICommandHandler<RemoveMenuItemImageCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveMenuItemImageCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var cleared = menu.SetItemImage(command.ItemId, null, clock.GetUtcNow());
        if (cleared.IsFailure)
            return cleared.Error!;

        await db.SaveChangesAsync(ct);
        if (cleared.Value is { } previous)
            await storage.DeleteAsync([previous], CancellationToken.None);

        return Result.Success();
    }
}
