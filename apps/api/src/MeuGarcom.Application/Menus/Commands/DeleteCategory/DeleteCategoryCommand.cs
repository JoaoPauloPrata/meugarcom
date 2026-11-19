using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.DeleteCategory;

/// <summary>Deletes the category and all of its items.</summary>
public sealed record DeleteCategoryCommand(Guid UnitId, Guid CategoryId) : ICommand<Result>;
