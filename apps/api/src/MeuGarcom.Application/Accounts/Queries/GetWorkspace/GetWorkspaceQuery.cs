using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Accounts.Queries.GetWorkspace;

/// <summary>Account, restaurant and units of the signed-in owner. Fails with account.not_found before onboarding.</summary>
public sealed record GetWorkspaceQuery : IQuery<Result<WorkspaceDto>>;

public sealed record WorkspaceDto(
    Guid AccountId,
    string OwnerName,
    Guid RestaurantId,
    string RestaurantName,
    string? CuisineType,
    IReadOnlyList<UnitDto> Units);

public sealed record UnitDto(Guid Id, string Name, string? City, string PublicCode, decimal ComboBudgetTolerance);
