using MeuGarcom.Domain.Accounts;

namespace MeuGarcom.Domain.Tests;

public class UnitChatSuggestionsTests
{
    private static Unit NewUnit()
    {
        var account = Account.Create(Guid.NewGuid(), "Owner", DateTimeOffset.UtcNow).Value;
        var restaurant = account.AddRestaurant("Restaurant", null, DateTimeOffset.UtcNow).Value;
        return restaurant.AddUnit("Unit", null, DateTimeOffset.UtcNow).Value;
    }

    [Fact]
    public void Suggestions_are_trimmed_and_default_to_the_chat_icon()
    {
        var unit = NewUnit();

        var result = unit.ChangeChatSuggestions([("  Tem opção vegana?  ", null), ("Qual o prato do dia?", "sparkles")]);

        Assert.True(result.IsSuccess);
        Assert.Equal([new ChatSuggestion("Tem opção vegana?", "chat"), new ChatSuggestion("Qual o prato do dia?", "sparkles")], unit.ChatSuggestions);
    }

    [Fact]
    public void Repeated_texts_are_kept_once()
    {
        var unit = NewUnit();

        unit.ChangeChatSuggestions([("Tem sobremesa?", "chat"), ("tem sobremesa?", "heart")]);

        Assert.Single(unit.ChatSuggestions);
    }

    [Fact]
    public void More_than_the_limit_is_rejected_and_keeps_the_previous_list()
    {
        var unit = NewUnit();
        unit.ChangeChatSuggestions([("A", null)]);

        var result = unit.ChangeChatSuggestions(Enumerable.Range(1, Unit.MaxChatSuggestions + 1).Select(i => ($"Pergunta {i}", (string?)null)));

        Assert.Equal(UnitErrors.TooManyChatSuggestions, result.Error);
        Assert.Equal("A", Assert.Single(unit.ChatSuggestions).Text);
    }

    [Theory]
    [InlineData("   ", null)]
    [InlineData("Ok", "pizza")]
    public void Blank_text_or_unknown_icon_is_rejected(string text, string? icon)
    {
        var result = NewUnit().ChangeChatSuggestions([(text, icon)]);

        Assert.True(result.IsFailure);
    }
}
