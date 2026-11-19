using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.RenameCategory;

public sealed class RenameCategoryHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<RenameCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(RenameCategoryCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var renamed = menu.RenameCategory(command.CategoryId, command.Name, clock.GetUtcNow());
        if (renamed.IsFailure)
            return renamed;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
