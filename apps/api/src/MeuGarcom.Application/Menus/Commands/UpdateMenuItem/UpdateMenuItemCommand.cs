using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.UpdateMenuItem;

public sealed record UpdateMenuItemCommand(Guid UnitId, Guid ItemId, MenuItemInput Item) : ICommand<Result>;
