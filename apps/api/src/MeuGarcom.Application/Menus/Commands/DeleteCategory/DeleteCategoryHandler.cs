using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.DeleteCategory;

public sealed class DeleteCategoryHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IMenuImageStorage images,
    TimeProvider clock)
    : ICommandHandler<DeleteCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var imagePaths = menu.Items
            .Where(i => i.CategoryId == command.CategoryId && i.ImagePath is not null)
            .Select(i => i.ImagePath!)
            .ToList();
        var removed = menu.RemoveCategory(command.CategoryId, clock.GetUtcNow());
        if (removed.IsFailure)
            return removed;

        await db.SaveChangesAsync(ct);
        if (imagePaths.Count > 0)
            await images.DeleteAsync(imagePaths, CancellationToken.None);

        return Result.Success();
    }
}
