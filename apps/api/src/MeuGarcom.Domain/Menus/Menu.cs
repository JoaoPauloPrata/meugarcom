using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Menus;

/// <summary>Aggregate root with the categories and items of one unit.</summary>
public sealed class Menu : Entity
{
    public const int MaxItems = 600;

    private readonly List<MenuCategory> _categories = [];
    private readonly List<MenuItem> _items = [];

    private Menu() { }

    public Guid UnitId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyList<MenuCategory> Categories => _categories.AsReadOnly();
    public IReadOnlyList<MenuItem> Items => _items.AsReadOnly();

    public static Menu CreateFor(Guid unitId, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        UnitId = unitId,
        CreatedAt = now,
        UpdatedAt = now,
    };

    public Result<MenuCategory> AddCategory(string name, DateTimeOffset now)
    {
        var normalized = NormalizeCategoryName(name);
        if (normalized.IsFailure)
            return normalized.Error!;
        if (FindCategoryByName(normalized.Value) is not null)
            return MenuErrors.CategoryNameDuplicated;

        var position = _categories.Count == 0 ? 0 : _categories.Max(c => c.Position) + 1;
        var category = MenuCategory.Create(Id, normalized.Value, position, now);
        _categories.Add(category);
        Touch(now);
        return category;
    }

    public Result RenameCategory(Guid categoryId, string name, DateTimeOffset now)
    {
        var category = _categories.FirstOrDefault(c => c.Id == categoryId);
        if (category is null)
            return MenuErrors.CategoryNotFound;

        var normalized = NormalizeCategoryName(name);
        if (normalized.IsFailure)
            return normalized.Error!;

        var existing = FindCategoryByName(normalized.Value);
        if (existing is not null && existing.Id != categoryId)
            return MenuErrors.CategoryNameDuplicated;

        category.Rename(normalized.Value);
        Touch(now);
        return Result.Success();
    }

    /// <summary>Removes the category together with its items.</summary>
    public Result RemoveCategory(Guid categoryId, DateTimeOffset now)
    {
        var category = _categories.FirstOrDefault(c => c.Id == categoryId);
        if (category is null)
            return MenuErrors.CategoryNotFound;

        _items.RemoveAll(i => i.CategoryId == categoryId);
        _categories.Remove(category);
        Touch(now);
        return Result.Success();
    }

    public Result<MenuItem> AddItem(Guid categoryId, MenuItemDetails details, DateTimeOffset now)
    {
        if (_categories.All(c => c.Id != categoryId))
            return MenuErrors.CategoryNotFound;
        if (_items.Count >= MaxItems)
            return MenuErrors.TooManyItems;

        var normalized = details.Normalize();
        if (normalized.IsFailure)
            return normalized.Error!;

        var item = MenuItem.Create(Id, categoryId, normalized.Value, NextItemPosition(categoryId), now);
        _items.Add(item);
        Touch(now);
        return item;
    }

    public Result UpdateItem(Guid itemId, Guid categoryId, MenuItemDetails details, DateTimeOffset now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return MenuErrors.ItemNotFound;
        if (_categories.All(c => c.Id != categoryId))
            return MenuErrors.CategoryNotFound;

        var normalized = details.Normalize();
        if (normalized.IsFailure)
            return normalized.Error!;

        if (item.CategoryId != categoryId)
            item.MoveTo(categoryId, NextItemPosition(categoryId), now);

        item.Apply(normalized.Value, now);
        Touch(now);
        return Result.Success();
    }

    /// <summary>
    /// The only way to change vegan / lactose-free / gluten-free. It must be called from a use
    /// case triggered by a person in the menu settings, never from an automated import.
    /// </summary>
    public Result SetItemDietaryAttributes(Guid itemId, DietaryAttributes dietary, DateTimeOffset now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return MenuErrors.ItemNotFound;

        item.SetDietary(dietary, now);
        Touch(now);
        return Result.Success();
    }

    public Result SetItemAvailability(Guid itemId, bool isAvailable, DateTimeOffset now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return MenuErrors.ItemNotFound;

        item.SetAvailability(isAvailable, now);
        Touch(now);
        return Result.Success();
    }

    public Result RemoveItem(Guid itemId, DateTimeOffset now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return MenuErrors.ItemNotFound;

        _items.Remove(item);
        Touch(now);
        return Result.Success();
    }

    /// <summary>
    /// Adds imported items, creating categories by name when needed. Imported items never carry
    /// dietary flags: they all start as "not confirmed" and must be reviewed by a person.
    /// </summary>
    public Result<int> AddImportedItems(IReadOnlyList<ImportedMenuItem> items, DateTimeOffset now)
    {
        if (items.Count == 0)
            return MenuErrors.ImportEmpty;
        if (_items.Count + items.Count > MaxItems)
            return MenuErrors.TooManyItems;

        var prepared = new List<(string Category, MenuItemDetails Details)>(items.Count);
        foreach (var imported in items)
        {
            var category = NormalizeCategoryName(imported.CategoryName);
            if (category.IsFailure)
                return category.Error!;

            var details = imported.ToDetails().Normalize();
            if (details.IsFailure)
                return details.Error!;

            prepared.Add((category.Value, details.Value));
        }

        foreach (var (categoryName, details) in prepared)
        {
            var category = FindCategoryByName(categoryName) ?? AddCategory(categoryName, now).Value;
            _items.Add(MenuItem.Create(Id, category.Id, details, NextItemPosition(category.Id), now));
        }

        Touch(now);
        return prepared.Count;
    }

    private MenuCategory? FindCategoryByName(string name) =>
        _categories.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

    private int NextItemPosition(Guid categoryId)
    {
        var inCategory = _items.Where(i => i.CategoryId == categoryId).ToList();
        return inCategory.Count == 0 ? 0 : inCategory.Max(i => i.Position) + 1;
    }

    private static Result<string> NormalizeCategoryName(string? name)
    {
        var trimmed = name?.Trim() ?? "";
        return trimmed.Length is 0 or > MenuCategory.MaxNameLength
            ? MenuErrors.CategoryNameInvalid
            : trimmed;
    }

    private void Touch(DateTimeOffset now) => UpdatedAt = now;
}
