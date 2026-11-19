using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Queries.GetMenu;

public sealed record MenuDto(Guid Id, IReadOnlyList<MenuCategoryDto> Categories);

public sealed record MenuCategoryDto(Guid Id, string Name, int Position, IReadOnlyList<MenuItemDto> Items);

public sealed record MenuItemDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    string? Ingredients,
    decimal Price,
    int? ServesPeople,
    string? Portion,
    bool IsSpicy,
    bool IsVegan,
    bool IsLactoseFree,
    bool IsGlutenFree,
    bool IsAvailable)
{
    public static MenuItemDto From(MenuItem item) => new(
        item.Id, item.CategoryId, item.Name, item.Description, item.Ingredients, item.Price,
        item.ServesPeople, item.Portion, item.IsSpicy, item.IsVegan, item.IsLactoseFree,
        item.IsGlutenFree, item.IsAvailable);
}

public static class MenuDtoMapper
{
    public static MenuDto ToDto(this Menu menu, bool onlyAvailable = false) => new(
        menu.Id,
        menu.Categories
            .OrderBy(c => c.Position).ThenBy(c => c.Name)
            .Select(c => new MenuCategoryDto(
                c.Id,
                c.Name,
                c.Position,
                menu.Items
                    .Where(i => i.CategoryId == c.Id && (!onlyAvailable || i.IsAvailable))
                    .OrderBy(i => i.Position).ThenBy(i => i.Name)
                    .Select(MenuItemDto.From)
                    .ToList()))
            .Where(c => !onlyAvailable || c.Items.Count > 0)
            .ToList());
}
