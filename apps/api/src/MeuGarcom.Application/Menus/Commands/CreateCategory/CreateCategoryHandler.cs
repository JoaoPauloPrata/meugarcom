using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.CreateCategory;

public sealed class CreateCategoryHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<CreateCategoryCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var category = menu.AddCategory(command.Name, clock.GetUtcNow());
        if (category.IsFailure)
            return category.Error!;

        await db.SaveChangesAsync(ct);
        return category.Value.Id;
    }
}
