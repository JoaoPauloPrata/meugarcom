using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Menus;

/// <summary>Group of items (pizzas, wines, desserts). Mutated only through its <see cref="Menu"/>.</summary>
public sealed class MenuCategory : Entity
{
    public const int MaxNameLength = 80;

    private MenuCategory() { }

    public Guid MenuId { get; private set; }
    public string Name { get; private set; } = null!;
    public int Position { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    internal static MenuCategory Create(Guid menuId, string name, int position, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        MenuId = menuId,
        Name = name,
        Position = position,
        CreatedAt = now,
    };

    internal void Rename(string name) => Name = name;
}
