using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Accounts;

/// <summary>Shortcut shown on the chat welcome screen. Tapping it sends <see cref="Text"/> as the customer's question.</summary>
public sealed record ChatSuggestion(string Text, string Icon)
{
    public const int MaxTextLength = 80;
    public const string DefaultIcon = "chat";

    /// <summary>Icon keys the chat knows how to draw.</summary>
    public static readonly IReadOnlySet<string> Icons = new HashSet<string>(StringComparer.Ordinal)
    {
        "chat", "sparkles", "heart", "wheatOff", "leaf", "wine", "cup", "tag",
    };

    public static Result<ChatSuggestion> Create(string text, string? icon)
    {
        var trimmed = text?.Trim() ?? "";
        if (trimmed.Length is 0 or > MaxTextLength)
            return UnitErrors.ChatSuggestionTextInvalid;

        var key = string.IsNullOrWhiteSpace(icon) ? DefaultIcon : icon.Trim();
        if (!Icons.Contains(key))
            return UnitErrors.ChatSuggestionIconInvalid;

        return new ChatSuggestion(trimmed, key);
    }
}
