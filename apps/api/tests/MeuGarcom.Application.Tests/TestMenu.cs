using MeuGarcom.Application.Conversations.Waiter;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Tests;

/// <summary>Small menu with the traps from docs/guardrails.md.</summary>
internal static class TestMenu
{
    public static MenuSnapshot Build()
    {
        var now = DateTimeOffset.UtcNow;
        var menu = Menu.CreateFor(Guid.NewGuid(), now);
        var mains = menu.AddCategory("Pratos principais", now).Value;
        var drinks = menu.AddCategory("Bebidas", now).Value;
        var sushi = menu.AddCategory("Sushi", now).Value;

        Add(menu, mains.Id, new("X-Cheese", "Hambúrguer da casa", "pão, carne, alface", 32m, null, null, false));
        Add(menu, mains.Id, new("Feijão tropeiro", null, "feijão, farinha, ovo, couve", 45m, 2, null, false));
        Add(menu, mains.Id, new("Moqueca", null, "peixe, leite de coco, pimentão", 89.9m, 2, null, true));
        var palmito = Add(menu, mains.Id, new("Moqueca de palmito", null, "palmito, leite de coco, pimentão", 72m, 2, null, false));
        menu.SetItemDietaryAttributes(palmito.Id, new DietaryAttributes(true, true, true), now);
        Add(menu, mains.Id, new("Torrada Sem Glúten", null, null, 10m, 1, null, false));
        Add(menu, mains.Id, new("Risoto de cogumelos", null, "arroz arbóreo, cogumelos, caldo de legumes", 38m, 1, null, false));
        Add(menu, drinks.Id, new("Suco de maracujá", null, "maracujá, água", 12m, null, "500 ml", false));
        Add(menu, drinks.Id, new("Coca-Cola lata", null, null, 7m, null, "350 ml", false));
        Add(menu, drinks.Id, new("Coca-Cola Zero lata", null, null, 7m, null, "350 ml", false));
        Add(menu, sushi.Id, new("Combinado 20 peças", "Sushis e sashimis variados", null, 89m, 2, "20 peças", false));
        Add(menu, sushi.Id, new("Temaki de salmão", null, "salmão, arroz, alga", 32m, 1, null, false));
        Add(menu, sushi.Id, new("Hot roll 10 peças", null, "salmão, cream cheese, arroz, alga", 34m, 1, "10 peças", false));

        return MenuSnapshot.Build(menu, "Bistrô Teste", 7, 0.20m);
    }

    private static MenuItem Add(Menu menu, Guid categoryId, MenuItemDetails details) =>
        menu.AddItem(categoryId, details, DateTimeOffset.UtcNow).Value;

    public static string RefOf(MenuSnapshot menu, string name) =>
        menu.ItemsByRef.Single(kv => kv.Value.Name == name).Key;
}
