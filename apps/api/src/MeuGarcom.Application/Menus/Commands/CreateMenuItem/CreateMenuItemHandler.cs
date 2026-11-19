using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.CreateMenuItem;

public sealed class CreateMenuItemHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<CreateMenuItemCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateMenuItemCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var now = clock.GetUtcNow();
        var item = menu.AddItem(command.Item.CategoryId, command.Item.ToDetails(), now);
        if (item.IsFailure)
            return item.Error!;

        // Manual creation by a person: the dietary checkboxes are a deliberate human decision.
        menu.SetItemDietaryAttributes(item.Value.Id, command.Item.ToDietary(), now);

        await db.SaveChangesAsync(ct);
        return item.Value.Id;
    }
}
