using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.DeleteMenuItem;

public sealed record DeleteMenuItemCommand(Guid UnitId, Guid ItemId) : ICommand<Result>;
