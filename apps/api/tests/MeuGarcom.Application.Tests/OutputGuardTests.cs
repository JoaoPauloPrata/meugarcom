using MeuGarcom.Application.Conversations.Waiter;

namespace MeuGarcom.Application.Tests;

public class OutputGuardTests
{
    private readonly MenuSnapshot _menu = TestMenu.Build();

    private OutputCheck Check(string reply, IEnumerable<string>? refs = null, WaiterComboOutput? combo = null, params string[] customer) =>
        OutputGuard.Check(new WaiterOutput(reply, refs?.ToList() ?? [], combo), _menu, customer);

    [Fact]
    public void Correct_price_of_a_cited_item_is_approved()
    {
        var check = Check("O risoto de cogumelos custa R$ 38,00.", [TestMenu.RefOf(_menu, "Risoto de cogumelos")]);

        Assert.True(check.IsApproved);
        Assert.Single(check.Reply!.ItemIds);
    }

    [Fact]
    public void Price_that_is_not_in_the_menu_is_rejected()
    {
        var check = Check("O risoto de cogumelos custa R$ 35,00.", [TestMenu.RefOf(_menu, "Risoto de cogumelos")]);

        Assert.False(check.IsApproved);
    }

    [Fact]
    public void Unknown_item_ref_is_rejected()
    {
        Assert.False(Check("Temos lasanha.", ["i999"]).IsApproved);
    }

    [Fact]
    public void Affirming_an_unconfirmed_dietary_attribute_is_rejected()
    {
        var check = Check("Sim, o suco de maracujá é vegano.", [TestMenu.RefOf(_menu, "Suco de maracujá")]);

        Assert.False(check.IsApproved);
    }

    [Fact]
    public void Sending_to_the_waiter_about_an_unconfirmed_attribute_is_approved()
    {
        var check = Check(
            "Não consigo confirmar se o suco de maracujá é vegano pelo cardápio. Chame o garçom, que ele tira essa dúvida.",
            [TestMenu.RefOf(_menu, "Suco de maracujá")]);

        Assert.True(check.IsApproved);
    }

    [Fact]
    public void Confirmed_vegan_item_can_be_affirmed()
    {
        var check = Check("Sim, a moqueca de palmito é vegana.", [TestMenu.RefOf(_menu, "Moqueca de palmito")]);

        Assert.True(check.IsApproved);
    }

    [Fact]
    public void Item_named_in_text_counts_even_when_not_cited()
    {
        Assert.False(Check("Sim, o Suco de maracujá é vegano.").IsApproved);
    }

    [Fact]
    public void Dietary_word_inside_an_item_name_is_not_a_claim()
    {
        var check = Check(
            "Temos X-Cheese por R$ 32,00 e Torrada Sem Glúten por R$ 10,00.",
            [TestMenu.RefOf(_menu, "X-Cheese"), TestMenu.RefOf(_menu, "Torrada Sem Glúten")]);

        Assert.True(check.IsApproved);
    }

    [Fact]
    public void Item_name_does_not_confirm_its_own_attribute()
    {
        Assert.False(Check("Sim, a Torrada Sem Glúten é sem glúten.").IsApproved);
    }

    [Fact]
    public void Category_without_confirmed_items_in_a_dietary_group_reply_is_rejected()
    {
        Assert.False(Check("Temos opções sem glúten em pratos principais e sushi.").IsApproved);
    }

    [Fact]
    public void Category_with_a_confirmed_item_in_a_dietary_group_reply_is_approved()
    {
        Assert.True(Check("Temos opções sem glúten em pratos principais. Quer que eu liste?").IsApproved);
    }

    [Fact]
    public void Menu_data_carries_confirmed_attribute_counts_per_category()
    {
        Assert.Contains("\"confirmedAttributeCounts\":{\"vegan\":1,\"lactose_free\":1,\"gluten_free\":1}", _menu.DataJson);
    }

    [Fact]
    public void Budget_repeated_from_the_customer_is_allowed()
    {
        var check = Check("Para R$ 150 separei estas opções:", [TestMenu.RefOf(_menu, "Combinado 20 peças")], null, "combo de sushi para dois de 150 reais");

        Assert.True(check.IsApproved);
    }

    [Fact]
    public void Combo_inside_range_is_computed_by_the_backend()
    {
        var combo = new WaiterComboOutput(150m, [new(TestMenu.RefOf(_menu, "Combinado 20 peças"), 1), new(TestMenu.RefOf(_menu, "Hot roll 10 peças"), 1)]);

        var check = Check("Montei um combo de sushi para dois.", [], combo, "combo de sushi para dois de 150 reais");

        Assert.True(check.IsApproved);
        Assert.Equal(123m, check.Reply!.Combo!.Total);
    }

    [Fact]
    public void Combo_outside_range_is_rejected()
    {
        var combo = new WaiterComboOutput(150m, [new(TestMenu.RefOf(_menu, "Combinado 20 peças"), 3)]);

        Assert.False(Check("Montei um combo.", [], combo).IsApproved);
    }

    [Fact]
    public void Combo_that_does_not_feed_the_party_is_rejected()
    {
        // Hot roll serves 1: a single one is not a main course for two.
        var combo = new WaiterComboOutput(40m, [new(TestMenu.RefOf(_menu, "Hot roll 10 peças"), 1, "main")], People: 2);

        var check = Check("Montei um combo para dois.", [], combo, "combo para dois de 40 reais");

        Assert.False(check.IsApproved);
        Assert.Contains(check.Problems, p => p.Contains("don't feed everyone"));
    }

    [Fact]
    public void Combo_with_enough_portions_for_the_party_is_approved()
    {
        var combo = new WaiterComboOutput(70m, [new(TestMenu.RefOf(_menu, "Hot roll 10 peças"), 2, "main")], People: 2);

        var check = Check("Montei um combo para dois.", [], combo, "combo para dois de 70 reais");

        Assert.True(check.IsApproved);
        Assert.Equal(2, check.Reply!.Combo!.People);
    }

    [Theory]
    [InlineData("Posso te dar 10% de desconto no risoto!")]
    [InlineData("Vou anotar seu pedido agora.")]
    [InlineData("O risoto fica pronto em 15 minutos.")]
    [InlineData("Veja mais em https://example.com")]
    public void Promises_and_links_are_rejected(string reply)
    {
        Assert.False(Check(reply).IsApproved);
    }

    [Fact]
    public void Declining_a_discount_is_approved()
    {
        Assert.True(Check("Não consigo oferecer desconto, mas o garçom pode ajudar com o que precisar.").IsApproved);
    }

    [Fact]
    public void Group_reply_keeps_its_quick_replies()
    {
        var check = OutputGuard.Check(
            new WaiterOutput("Temos drinks, cervejas e vinhos. Qual você prefere?", [], null,
                ["Liste os drinks", "Liste as cervejas", "Liste os vinhos"]),
            _menu, []);

        Assert.True(check.IsApproved);
        Assert.Equal(["Liste os drinks", "Liste as cervejas", "Liste os vinhos"], check.Reply!.QuickReplies);
    }

    [Fact]
    public void Invalid_quick_replies_are_dropped_without_rejecting_the_answer()
    {
        var check = OutputGuard.Check(
            new WaiterOutput("Temos cervejas e vinhos.", [], null,
            [
                "  Liste   as cervejas ",
                "liste as cervejas",
                "Veja https://example.com",
                "",
                new string('a', OutputGuard.MaxQuickReplyLength + 1),
                "Liste os vinhos",
                "Quais são sem glúten?",
                "Tem sobremesa?",
                "Tem café?",
            ]),
            _menu, []);

        Assert.True(check.IsApproved);
        Assert.Equal(["Liste as cervejas", "Liste os vinhos", "Quais são sem glúten?", "Tem sobremesa?"], check.Reply!.QuickReplies);
    }

    [Fact]
    public void Missing_quick_replies_become_an_empty_list()
    {
        var check = Check("Temos cervejas e vinhos.");

        Assert.True(check.IsApproved);
        Assert.Empty(check.Reply!.QuickReplies);
    }
}
