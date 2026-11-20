namespace MeuGarcom.Application.Common.Messages;

/// <summary>Welcome suggestions (PT-BR) a new unit starts with. The owner can change them in the settings.</summary>
public static class DefaultChatSuggestions
{
    public static readonly IReadOnlyList<(string Text, string? Icon)> All =
    [
        ("O que você recomenda?", "chat"),
        ("Quais opções são sem glúten?", "wheatOff"),
        ("Que vinho combina com os pratos principais?", "wine"),
    ];
}
