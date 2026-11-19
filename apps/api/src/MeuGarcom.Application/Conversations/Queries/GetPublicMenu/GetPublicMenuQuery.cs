using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Menus.Queries.GetMenu;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Conversations;

namespace MeuGarcom.Application.Conversations.Queries.GetPublicMenu;

/// <summary>Full menu shown in the chat ("Ver cardápio completo"). Only available items.</summary>
public sealed record GetPublicMenuQuery(string AccessCode) : IQuery<Result<PublicMenuDto>>;

public sealed record PublicMenuDto(string RestaurantName, IReadOnlyList<MenuCategoryDto> Categories);
