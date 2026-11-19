using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Menus.Queries.GetMenu;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Conversations;

namespace MeuGarcom.Application.Conversations.Queries.GetPublicMenu;

public sealed class GetPublicMenuHandler(IAppDbContext db) : IQueryHandler<GetPublicMenuQuery, Result<PublicMenuDto>>
{
    public async ValueTask<Result<PublicMenuDto>> Handle(GetPublicMenuQuery query, CancellationToken ct)
    {
        var access = await db.ResolveAsync(query.AccessCode, ct);
        if (access is null)
            return ConversationErrors.AccessCodeInvalid;

        var menu = await db.LoadMenuForUnitAsync(access.UnitId, ct);
        var categories = menu?.ToDto(onlyAvailable: true).Categories ?? [];
        return new PublicMenuDto(access.RestaurantName, categories);
    }
}
