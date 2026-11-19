using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.PublishMenuImport;

/// <summary>Publishes the items reviewed by a person. There are no dietary fields here on purpose.</summary>
public sealed record PublishMenuImportCommand(Guid UnitId, Guid ImportId, IReadOnlyList<ReviewedItem> Items)
    : ICommand<Result<int>>;

public sealed record ReviewedItem(
    string CategoryName,
    string Name,
    string? Description,
    string? Ingredients,
    decimal Price,
    int? ServesPeople,
    string? Portion,
    bool IsSpicy);
