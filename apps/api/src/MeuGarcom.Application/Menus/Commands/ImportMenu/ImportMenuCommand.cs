using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.ImportMenu;

/// <summary>Extracts a draft menu from a file (PDF, image or spreadsheet) for manual review.</summary>
public sealed record ImportMenuCommand(Guid UnitId, string FileName, string ContentType, byte[] Content)
    : ICommand<Result<MenuImportDto>>;

public sealed record MenuImportDto(Guid Id, string FileName, IReadOnlyList<ImportedItemDto> Items);

/// <param name="NeedsReview">Price missing or text that looks like an instruction (possible injection).</param>
public sealed record ImportedItemDto(
    string CategoryName,
    string Name,
    string? Description,
    string? Ingredients,
    decimal Price,
    int? ServesPeople,
    string? Portion,
    bool IsSpicy,
    bool NeedsReview);
