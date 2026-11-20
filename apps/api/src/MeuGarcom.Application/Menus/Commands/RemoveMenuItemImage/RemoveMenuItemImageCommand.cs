using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.RemoveMenuItemImage;

public sealed record RemoveMenuItemImageCommand(Guid UnitId, Guid ItemId) : ICommand<Result>;
