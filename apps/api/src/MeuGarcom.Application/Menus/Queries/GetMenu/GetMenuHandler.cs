using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Menus.Queries.GetMenu;

public sealed class GetMenuHandler(IAppDbContext db, ICurrentUser currentUser, IMenuImageStorage images)
    : IQueryHandler<GetMenuQuery, Result<MenuDto>>
{
    public async ValueTask<Result<MenuDto>> Handle(GetMenuQuery query, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, query.UnitId, ct, tracking: false);
        return menu is null ? MenuErrors.MenuNotFound : menu.ToDto(images);
    }
}
