using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Menus;

public enum MenuImportStatus
{
    Draft,
    Published,
    Discarded,
}

/// <summary>
/// Item extracted from an uploaded menu file. It has no dietary flags on purpose: the import
/// can never mark an item as vegan, lactose-free or gluten-free.
/// </summary>
public sealed record ImportedMenuItem(
    string CategoryName,
    string Name,
    string? Description,
    string? Ingredients,
    decimal Price,
    int? ServesPeople,
    string? Portion,
    bool IsSpicy)
{
    public MenuItemDetails ToDetails() =>
        new(Name, Description, Ingredients, Price, ServesPeople, Portion, IsSpicy);
}

/// <summary>Extraction of a menu from a file, kept as a draft until a person reviews and publishes it.</summary>
public sealed class MenuImport : Entity
{
    public const int MaxFileNameLength = 255;

    private MenuImport() { }

    public Guid UnitId { get; private set; }
    public string FileName { get; private set; } = null!;
    public MenuImportStatus Status { get; private set; }
    public IReadOnlyList<ImportedMenuItem> Draft { get; private set; } = [];
    public int ItemCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }

    public static MenuImport Create(Guid unitId, string fileName, IReadOnlyList<ImportedMenuItem> draft, DateTimeOffset now)
    {
        var name = string.IsNullOrWhiteSpace(fileName) ? "menu" : fileName.Trim();
        if (name.Length > MaxFileNameLength)
            name = name[..MaxFileNameLength];

        return new MenuImport
        {
            Id = Guid.CreateVersion7(),
            UnitId = unitId,
            FileName = name,
            Status = MenuImportStatus.Draft,
            Draft = draft,
            ItemCount = draft.Count,
            CreatedAt = now,
        };
    }

    /// <summary>Publishes the reviewed items (the person may have edited the draft) into the menu.</summary>
    public Result<int> PublishInto(Menu menu, IReadOnlyList<ImportedMenuItem> reviewedItems, DateTimeOffset now)
    {
        if (Status != MenuImportStatus.Draft)
            return MenuErrors.ImportNotDraft;

        var added = menu.AddImportedItems(reviewedItems, now);
        if (added.IsFailure)
            return added.Error!;

        Status = MenuImportStatus.Published;
        ItemCount = added.Value;
        PublishedAt = now;
        return added.Value;
    }

    public Result Discard()
    {
        if (Status != MenuImportStatus.Draft)
            return MenuErrors.ImportNotDraft;

        Status = MenuImportStatus.Discarded;
        return Result.Success();
    }
}
