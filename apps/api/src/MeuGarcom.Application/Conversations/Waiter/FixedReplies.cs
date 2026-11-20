using System.Globalization;

namespace MeuGarcom.Application.Conversations.Waiter;

public enum FixedReply
{
    Blocked,
    Abuse,
    AbuseClosed,
    SafeFallback,
    ComboOverBudget,
}

/// <summary>
/// Catalog of fixed replies that never go through the model (guardrail blocks and the safe
/// reply). Chosen by the language stored in the conversation; English is the reserve language.
/// </summary>
public static class FixedReplies
{
    private static readonly CultureInfo Brazil = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly Dictionary<string, Dictionary<FixedReply, string>> Catalog = new()
    {
        ["pt"] = new()
        {
            [FixedReply.Blocked] = "Posso ajudar só com o cardápio da casa. Quer saber algo sobre os pratos ou as bebidas?",
            [FixedReply.Abuse] = "Prefiro manter nossa conversa sobre o cardápio. Se precisar de algo além disso, o garçom pode ajudar.",
            [FixedReply.AbuseClosed] = "Vou encerrar a conversa por aqui. Se precisar de algo, é só chamar o garçom.",
            [FixedReply.SafeFallback] = "Desculpe, não consegui responder isso com segurança agora. Pode perguntar de outro jeito ou chamar o garçom?",
            [FixedReply.ComboOverBudget] = "Não consegui montar dentro de {0}, mas esta opção fica em {1}.",
        },
        ["en"] = new()
        {
            [FixedReply.Blocked] = "I can only help with the menu. Would you like to know anything about the dishes or drinks?",
            [FixedReply.Abuse] = "I'd rather keep our chat about the menu. For anything else, your waiter can help.",
            [FixedReply.AbuseClosed] = "I'll end the chat here. If you need anything, just call your waiter.",
            [FixedReply.SafeFallback] = "Sorry, I couldn't answer that reliably right now. Could you rephrase it or ask your waiter?",
            [FixedReply.ComboOverBudget] = "I couldn't put it together within {0}, but this option comes to {1}.",
        },
        ["es"] = new()
        {
            [FixedReply.Blocked] = "Solo puedo ayudarte con el menú. ¿Quieres saber algo sobre los platos o las bebidas?",
            [FixedReply.Abuse] = "Prefiero que hablemos del menú. Para cualquier otra cosa, el camarero puede ayudarte.",
            [FixedReply.AbuseClosed] = "Voy a terminar la conversación aquí. Si necesitas algo, llama al camarero.",
            [FixedReply.SafeFallback] = "Disculpa, no pude responder eso con seguridad. ¿Puedes preguntarlo de otra forma o llamar al camarero?",
            [FixedReply.ComboOverBudget] = "No logré armarlo con {0}, pero esta opción sale por {1}.",
        },
    };

    public static string Get(FixedReply reply, string? language, params object[] args)
    {
        var template = Catalog[CatalogKey(language)][reply];
        return args.Length == 0 ? template : string.Format(Brazil, template, args);
    }

    /// <summary>Prices are always shown in BRL with the Brazilian format, whatever the language.</summary>
    public static string FormatBrl(decimal value) => value.ToString("C", Brazil);

    private static string CatalogKey(string? language)
    {
        var tag = (language ?? "pt").Trim().ToLowerInvariant();
        if (tag.StartsWith("pt")) return "pt";
        if (tag.StartsWith("es")) return "es";
        return "en";
    }
}
