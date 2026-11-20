using System.Text.RegularExpressions;
using MeuGarcom.Domain.Ai;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeuGarcom.Infrastructure.Persistence;

/// <summary>Stores enums as the snake_case text values used by the SQL check constraints.</summary>
public static partial class EnumText<TEnum> where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> ToText = Enum.GetValues<TEnum>().ToDictionary(v => v, Name);
    private static readonly Dictionary<string, TEnum> FromText = ToText.ToDictionary(kv => kv.Value, kv => kv.Key);

    public static string ToDb(TEnum value) => ToText[value];

    public static TEnum FromDb(string value) => FromText[value];

    public static ValueConverter<TEnum, string> Converter { get; } = new(v => ToDb(v), v => FromDb(v));

    private static string Name(TEnum value) => value is AiProvider.OpenAi
        ? "openai"
        : SnakeCase().Replace(value.ToString(), "$1_$2").ToLowerInvariant();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex SnakeCase();
}
