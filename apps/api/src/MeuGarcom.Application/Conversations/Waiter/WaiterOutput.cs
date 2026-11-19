using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeuGarcom.Application.Conversations.Waiter;

/// <summary>Structured answer of the model (fixed schema in <see cref="WaiterPrompt.Schema"/>).</summary>
public sealed record WaiterOutput(
    [property: JsonPropertyName("reply")] string? Reply,
    [property: JsonPropertyName("referencedItemRefs")] List<string>? ReferencedItemRefs,
    [property: JsonPropertyName("combo")] WaiterComboOutput? Combo)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static WaiterOutput? TryParse(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<WaiterOutput>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

public sealed record WaiterComboOutput(
    [property: JsonPropertyName("budget")] decimal Budget,
    [property: JsonPropertyName("items")] List<WaiterComboItemOutput>? Items);

public sealed record WaiterComboItemOutput(
    [property: JsonPropertyName("ref")] string? Ref,
    [property: JsonPropertyName("quantity")] int Quantity);
