using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.RenameCategory;

public sealed record RenameCategoryCommand(Guid UnitId, Guid CategoryId, string Name) : ICommand<Result>;
