using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MeuGarcom.Application.Common.Guardrails;

/// <summary>
/// Cheap deterministic filter for well-known prompt-injection / jailbreak phrasings (OWASP LLM01).
/// It is the first layer only: the model classifier and the output guardrail come after it.
/// Used on customer messages and on text extracted from imported menus (indirect injection).
/// </summary>
public static partial class InjectionPatterns
{
    private static readonly Regex[] Patterns =
    [
        IgnoreInstructions(),
        RevealPrompt(),
        RoleOverride(),
        JailbreakKeywords(),
        FakeMarkup(),
        FakeAuthority(),
    ];

    public static bool IsSuspicious(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = Normalize(text);
        return Patterns.Any(p => p.IsMatch(normalized));
    }

    /// <summary>Lowercase, without diacritics and with collapsed whitespace.</summary>
    private static string Normalize(string text)
    {
        var decomposed = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.IsWhiteSpace(c) ? ' ' : c);
        }
        return Whitespace().Replace(builder.ToString(), " ");
    }

    [GeneratedRegex(@"\b(ignor\w*|esque[cç]\w*|desconsider\w*|forget|disregard|override|olvid\w*)\b.{0,40}\b(instru\w*|regras?|rules?|prompt|diretrizes|guidelines|instrucciones|reglas)\b")]
    private static partial Regex IgnoreInstructions();

    [GeneratedRegex(@"\b(system prompt|prompt (de|do) sistema|prompt del sistema|suas instrucoes|your instructions|tus instrucciones|initial prompt|prompt inicial|mensagem de sistema|developer message)\b")]
    private static partial Regex RevealPrompt();

    [GeneratedRegex(@"\b(you are now|from now on you|agora voce e|a partir de agora voce|ahora eres|finja que voce|finja ser|pretend (you are|to be)|act as|aja como|actua como|roleplay)\b")]
    private static partial Regex RoleOverride();

    [GeneratedRegex(@"\b(jailbreak|dan mode|developer mode|modo desenvolvedor|modo desarrollador|do anything now|sem restricoes|without restrictions|sin restricciones)\b")]
    private static partial Regex JailbreakKeywords();

    [GeneratedRegex(@"(<\/?\s*(system|assistant|instructions?|menu_data)\s*>|\[/?inst\]|<\|im_start\|>|###\s*(system|instruction))")]
    private static partial Regex FakeMarkup();

    [GeneratedRegex(@"\b(o dono|o gerente|the owner|the manager|el dueno|el gerente)\b.{0,30}\b(autoriz\w*|liber\w*|authori[sz]\w*|allow\w*|permit\w*)\b")]
    private static partial Regex FakeAuthority();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
