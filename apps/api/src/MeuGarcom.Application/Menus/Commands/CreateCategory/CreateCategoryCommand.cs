using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.CreateCategory;

public sealed record CreateCategoryCommand(Guid UnitId, string Name) : ICommand<Result<Guid>>;
