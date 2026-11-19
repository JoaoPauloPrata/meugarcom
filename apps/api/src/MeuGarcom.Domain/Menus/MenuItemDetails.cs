using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Menus;

/// <summary>
/// Descriptive fields of a menu item. Dietary flags are deliberately absent: they are only
/// changed through <see cref="Menu.SetItemDietaryAttributes"/>, which is a manual action.
/// </summary>
public sealed record MenuItemDetails(
    string Name,
    string? Description,
    string? Ingredients,
    decimal Price,
    int? ServesPeople,
    string? Portion,
    bool IsSpicy)
{
    public const int MaxNameLength = 120;
    public const int MaxTextLength = 1000;
    public const int MaxPortionLength = 60;
    public const decimal MaxPrice = 99_999m;

    public Result<MenuItemDetails> Normalize()
    {
        var name = Name?.Trim() ?? "";
        if (name.Length is 0 or > MaxNameLength)
            return MenuErrors.ItemNameInvalid;

        var description = Clean(Description);
        var ingredients = Clean(Ingredients);
        if (description is { Length: > MaxTextLength } || ingredients is { Length: > MaxTextLength })
            return MenuErrors.ItemTextTooLong;

        var portion = Clean(Portion);
        if (portion is { Length: > MaxPortionLength })
            return MenuErrors.ItemPortionInvalid;

        if (Price < 0 || Price > MaxPrice || decimal.Round(Price, 2) != Price)
            return MenuErrors.ItemPriceInvalid;

        if (ServesPeople is < 1 or > 50)
            return MenuErrors.ItemServesPeopleInvalid;

        return this with
        {
            Name = name,
            Description = description,
            Ingredients = ingredients,
            Portion = portion,
        };
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record DietaryAttributes(bool IsVegan, bool IsLactoseFree, bool IsGlutenFree)
{
    public static readonly DietaryAttributes None = new(false, false, false);
}
