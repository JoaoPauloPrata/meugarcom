using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.CreateMenuItem;

public sealed record CreateMenuItemCommand(Guid UnitId, MenuItemInput Item) : ICommand<Result<Guid>>;
