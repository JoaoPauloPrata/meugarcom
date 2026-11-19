using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Accounts.Queries.GetWorkspace;

public sealed class GetWorkspaceHandler(IAppDbContext db, ICurrentUser currentUser)
    : IQueryHandler<GetWorkspaceQuery, Result<WorkspaceDto>>
{
    public async ValueTask<Result<WorkspaceDto>> Handle(GetWorkspaceQuery query, CancellationToken ct)
    {
        var account = await db.Accounts
            .AsNoTracking()
            .Include(a => a.Restaurants).ThenInclude(r => r.Units)
            .FirstOrDefaultAsync(a => a.OwnerUserId == currentUser.UserId, ct);

        var restaurant = account?.Restaurants.OrderBy(r => r.CreatedAt).FirstOrDefault();
        if (account is null || restaurant is null)
            return AccountErrors.NotFound;

        return new WorkspaceDto(
            account.Id,
            account.Name,
            restaurant.Id,
            restaurant.Name,
            restaurant.CuisineType,
            restaurant.Units
                .OrderBy(u => u.CreatedAt)
                .Select(u => new UnitDto(u.Id, u.Name, u.City, u.PublicCode, u.ComboBudgetTolerance))
                .ToList());
    }
}
