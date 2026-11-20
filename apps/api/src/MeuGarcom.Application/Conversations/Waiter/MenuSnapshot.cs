using System.Text.Json;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Conversations.Waiter;

/// <summary>
/// The unit's menu as the single source of truth for one chat turn. Items get short refs
/// ("i1", "i2"...) so the model cites them reliably; the backend maps refs back to ids, prices
/// and flags for the output guardrail.
/// </summary>
public sealed class MenuSnapshot
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private MenuSnapshot(
        IReadOnlyDictionary<string, MenuItem> itemsByRef,
        decimal comboBudgetTolerance,
        string dataJson,
        string searchableText)
    {
        ItemsByRef = itemsByRef;
        ComboBudgetTolerance = comboBudgetTolerance;
        DataJson = dataJson;
        SearchableText = searchableText;
        RefById = itemsByRef.ToDictionary(kv => kv.Value.Id, kv => kv.Key);
    }

    public IReadOnlyDictionary<string, MenuItem> ItemsByRef { get; }
    public IReadOnlyDictionary<Guid, string> RefById { get; }
    public decimal ComboBudgetTolerance { get; }

    /// <summary>Menu serialized as JSON, sent to the model inside &lt;menu_data&gt; tags.</summary>
    public string DataJson { get; }

    /// <summary>Lowercase text of every item, used to accept terms that the menu itself contains.</summary>
    public string SearchableText { get; }

    public IEnumerable<MenuItem> Items => ItemsByRef.Values;

    public static MenuSnapshot Build(Menu? menu, string restaurantName, int? tableNumber, decimal comboBudgetTolerance)
    {
        var itemsByRef = new Dictionary<string, MenuItem>();
        var categories = new List<object>();
        var counter = 0;

        foreach (var category in (menu?.Categories ?? []).OrderBy(c => c.Position).ThenBy(c => c.Name))
        {
            var items = new List<object>();
            foreach (var item in menu!.Items
                         .Where(i => i.CategoryId == category.Id && i.IsAvailable)
                         .OrderBy(i => i.Position).ThenBy(i => i.Name))
            {
                var itemRef = $"i{++counter}";
                itemsByRef[itemRef] = item;
                items.Add(new
                {
                    Ref = itemRef,
                    item.Name,
                    Price = FixedReplies.FormatBrl(item.Price),
                    item.Description,
                    item.Ingredients,
                    item.Portion,
                    item.ServesPeople,
                    ConfirmedAttributes = ConfirmedAttributes(item),
                });
            }

            if (items.Count > 0)
                categories.Add(new { Category = category.Name, Items = items });
        }

        var data = new
        {
            Restaurant = restaurantName,
            Table = tableNumber,
            Currency = "BRL",
            ComboBudgetTolerancePercent = (int)(comboBudgetTolerance * 100),
            Categories = categories,
        };

        var searchable = string.Join('\n', itemsByRef.Values.Select(i =>
            $"{i.Name} {i.Description} {i.Ingredients} {i.Portion}")).ToLowerInvariant();

        return new MenuSnapshot(itemsByRef, comboBudgetTolerance, JsonSerializer.Serialize(data, JsonOptions), searchable);
    }

    /// <summary>Only attributes explicitly marked true. Anything absent is "not informed", never "no".</summary>
    private static List<string> ConfirmedAttributes(MenuItem item)
    {
        var attributes = new List<string>(4);
        if (item.IsVegan) attributes.Add("vegan");
        if (item.IsLactoseFree) attributes.Add("lactose_free");
        if (item.IsGlutenFree) attributes.Add("gluten_free");
        if (item.IsSpicy) attributes.Add("spicy");
        return attributes;
    }
}
