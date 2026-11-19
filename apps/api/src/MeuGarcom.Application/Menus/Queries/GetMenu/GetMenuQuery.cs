using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Queries.GetMenu;

public sealed record GetMenuQuery(Guid UnitId) : IQuery<Result<MenuDto>>;
