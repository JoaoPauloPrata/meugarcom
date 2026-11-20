using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Menus.Commands.PublishMenuImport;

public sealed class PublishMenuImportHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<PublishMenuImportCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(PublishMenuImportCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;

        var import = await db.MenuImports.FirstOrDefaultAsync(i => i.Id == command.ImportId && i.UnitId == command.UnitId, ct);
        if (import is null)
            return MenuErrors.ImportNotFound;

        var reviewed = command.Items
            .Select(i => new ImportedMenuItem(i.CategoryName, i.Name, i.Description, i.Ingredients, i.Price, i.ServesPeople, i.Portion, i.IsSpicy))
            .ToList();

        var published = import.PublishInto(menu, reviewed, clock.GetUtcNow());
        if (published.IsFailure)
            return published.Error!;

        await db.SaveChangesAsync(ct);
        return published.Value;
    }
}
