using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Menus;

/// <summary>Dish or drink. Mutated only through its <see cref="Menu"/> aggregate.</summary>
public sealed class MenuItem : Entity
{
    private MenuItem() { }

    public Guid MenuId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Ingredients { get; private set; }
    public decimal Price { get; private set; }
    public int? ServesPeople { get; private set; }
    public string? Portion { get; private set; }
    public bool IsSpicy { get; private set; }

    /// <summary>Manual only. True means the waiter may affirm it; false means "not confirmed".</summary>
    public bool IsVegan { get; private set; }

    /// <summary>Manual only. True means the waiter may affirm it; false means "not confirmed".</summary>
    public bool IsLactoseFree { get; private set; }

    /// <summary>Manual only. True means the waiter may affirm it; false means "not confirmed".</summary>
    public bool IsGlutenFree { get; private set; }

    public bool IsAvailable { get; private set; }
    public int Position { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public DietaryAttributes Dietary => new(IsVegan, IsLactoseFree, IsGlutenFree);

    internal static MenuItem Create(Guid menuId, Guid categoryId, MenuItemDetails details, int position, DateTimeOffset now)
    {
        var item = new MenuItem
        {
            Id = Guid.CreateVersion7(),
            MenuId = menuId,
            CategoryId = categoryId,
            IsAvailable = true,
            Position = position,
            CreatedAt = now,
        };
        item.Apply(details, now);
        return item;
    }

    internal void Apply(MenuItemDetails details, DateTimeOffset now)
    {
        Name = details.Name;
        Description = details.Description;
        Ingredients = details.Ingredients;
        Price = details.Price;
        ServesPeople = details.ServesPeople;
        Portion = details.Portion;
        IsSpicy = details.IsSpicy;
        UpdatedAt = now;
    }

    internal void MoveTo(Guid categoryId, int position, DateTimeOffset now)
    {
        CategoryId = categoryId;
        Position = position;
        UpdatedAt = now;
    }

    internal void SetDietary(DietaryAttributes dietary, DateTimeOffset now)
    {
        IsVegan = dietary.IsVegan;
        IsLactoseFree = dietary.IsLactoseFree;
        IsGlutenFree = dietary.IsGlutenFree;
        UpdatedAt = now;
    }

    internal void SetAvailability(bool isAvailable, DateTimeOffset now)
    {
        IsAvailable = isAvailable;
        UpdatedAt = now;
    }
}
