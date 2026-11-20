using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Domain.Tests;

public class MenuTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    public void Imported_items_never_carry_dietary_flags()
    {
        var menu = Menu.CreateFor(Guid.NewGuid(), Now);

        var added = menu.AddImportedItems(
            [new ImportedMenuItem("Sucos", "Suco de maracujá", "Fruta e água", null, 12m, null, "500 ml", false)], Now);

        Assert.True(added.IsSuccess);
        var item = Assert.Single(menu.Items);
        Assert.False(item.IsVegan);
        Assert.False(item.IsLactoseFree);
        Assert.False(item.IsGlutenFree);
        Assert.Equal("Sucos", Assert.Single(menu.Categories).Name);
    }

    [Fact]
    public void Imported_items_reuse_existing_category_ignoring_case()
    {
        var menu = Menu.CreateFor(Guid.NewGuid(), Now);
        menu.AddCategory("Pizzas", Now);

        menu.AddImportedItems([new ImportedMenuItem("pizzas", "Margherita", null, null, 50m, null, null, false)], Now);

        Assert.Single(menu.Categories);
    }

    [Fact]
    public void Dietary_flags_change_only_through_the_manual_method()
    {
        var menu = Menu.CreateFor(Guid.NewGuid(), Now);
        var category = menu.AddCategory("Pratos", Now).Value;
        var item = menu.AddItem(category.Id, new MenuItemDetails("Moqueca de palmito", null, null, 72m, 2, null, false), Now).Value;

        menu.SetItemDietaryAttributes(item.Id, new DietaryAttributes(true, false, false), Now);

        Assert.True(item.IsVegan);
        Assert.False(item.IsGlutenFree);
    }

    [Fact]
    public void Duplicated_category_name_is_rejected()
    {
        var menu = Menu.CreateFor(Guid.NewGuid(), Now);
        menu.AddCategory("Vinhos", Now);

        Assert.Equal(MenuErrors.CategoryNameDuplicated, menu.AddCategory(" vinhos ", Now).Error);
    }

    [Fact]
    public void Price_with_more_than_two_decimals_is_rejected()
    {
        var menu = Menu.CreateFor(Guid.NewGuid(), Now);
        var category = menu.AddCategory("Pratos", Now).Value;

        var item = menu.AddItem(category.Id, new MenuItemDetails("Risoto", null, null, 38.999m, null, null, false), Now);

        Assert.Equal(MenuErrors.ItemPriceInvalid, item.Error);
    }

    [Fact]
    public void Removing_a_category_removes_its_items()
    {
        var menu = Menu.CreateFor(Guid.NewGuid(), Now);
        var category = menu.AddCategory("Sobremesas", Now).Value;
        menu.AddItem(category.Id, new MenuItemDetails("Pudim", null, null, 18m, null, null, false), Now);

        menu.RemoveCategory(category.Id, Now);

        Assert.Empty(menu.Items);
        Assert.Empty(menu.Categories);
    }

    [Fact]
    public void Item_image_is_optional_and_replacing_it_returns_the_previous_path()
    {
        var menu = Menu.CreateFor(Guid.NewGuid(), Now);
        var category = menu.AddCategory("Pratos", Now).Value;
        var item = menu.AddItem(category.Id, new MenuItemDetails("Peixe grelhado", null, null, 64m, null, null, false), Now).Value;
        Assert.Null(item.ImagePath);

        Assert.Null(menu.SetItemImage(item.Id, "unit/first.webp", Now).Value);
        var replaced = menu.SetItemImage(item.Id, "unit/second.webp", Now);
        var cleared = menu.SetItemImage(item.Id, null, Now);

        Assert.Equal("unit/first.webp", replaced.Value);
        Assert.Equal("unit/second.webp", cleared.Value);
        Assert.Null(item.ImagePath);
    }

    [Fact]
    public void Item_image_rejects_unknown_item_and_blank_path()
    {
        var menu = Menu.CreateFor(Guid.NewGuid(), Now);
        var category = menu.AddCategory("Pratos", Now).Value;
        var item = menu.AddItem(category.Id, new MenuItemDetails("Peixe grelhado", null, null, 64m, null, null, false), Now).Value;

        Assert.Equal(MenuErrors.ItemNotFound, menu.SetItemImage(Guid.NewGuid(), "x.webp", Now).Error);
        Assert.Equal(MenuErrors.ItemImageInvalid, menu.SetItemImage(item.Id, " ", Now).Error);
    }
}
