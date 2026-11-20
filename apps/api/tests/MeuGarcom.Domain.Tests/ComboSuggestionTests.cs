using MeuGarcom.Domain.Combos;

namespace MeuGarcom.Domain.Tests;

public class ComboSuggestionTests
{
    private static ComboLine Line(decimal price, int quantity = 1) => new(Guid.NewGuid(), "Item", price, quantity);

    [Fact]
    public void Total_within_range_and_below_budget_is_valid()
    {
        var combo = ComboSuggestion.Create(150m, 0.20m, [Line(60m), Line(40m, 2)]);

        Assert.True(combo.IsSuccess);
        Assert.Equal(140m, combo.Value.Total);
        Assert.False(combo.Value.IsOverBudget);
    }

    [Fact]
    public void Total_above_budget_but_within_tolerance_is_flagged_over_budget()
    {
        var combo = ComboSuggestion.Create(150m, 0.20m, [Line(155m)]);

        Assert.True(combo.IsSuccess);
        Assert.True(combo.Value.IsOverBudget);
    }

    [Fact]
    public void Total_above_range_is_rejected()
    {
        var combo = ComboSuggestion.Create(150m, 0.20m, [Line(181m)]);

        Assert.Equal(ComboErrors.AboveRange, combo.Error);
    }

    [Fact]
    public void Total_below_range_is_rejected()
    {
        var combo = ComboSuggestion.Create(150m, 0.20m, [Line(119m)]);

        Assert.Equal(ComboErrors.BelowRange, combo.Error);
    }

    [Fact]
    public void Duplicated_items_are_rejected()
    {
        var id = Guid.NewGuid();
        var combo = ComboSuggestion.Create(100m, 0.20m, [new ComboLine(id, "A", 50m, 1), new ComboLine(id, "A", 50m, 1)]);

        Assert.Equal(ComboErrors.DuplicatedItem, combo.Error);
    }

    private static ComboLine Dish(decimal price, ComboCourse course, int? servesPeople, int quantity = 1) =>
        new(Guid.NewGuid(), "Item", price, quantity, course, servesPeople);

    [Fact]
    public void Single_portion_main_for_two_people_is_rejected()
    {
        var combo = ComboSuggestion.Create(140m, 0.50m,
            [Dish(34.90m, ComboCourse.Starter, 2), Dish(54.90m, ComboCourse.Main, 1)], people: 2);

        Assert.Equal(ComboErrors.NotEnoughForParty, combo.Error);
    }

    [Fact]
    public void Each_course_feeding_the_party_is_valid()
    {
        var combo = ComboSuggestion.Create(140m, 0.20m,
            [Dish(34.90m, ComboCourse.Starter, 2), Dish(54.90m, ComboCourse.Main, 1, quantity: 2)], people: 2);

        Assert.True(combo.IsSuccess);
        Assert.Equal(144.70m, combo.Value.Total);
        Assert.True(combo.Value.IsOverBudget);
        Assert.Equal(2, combo.Value.People);
    }

    [Fact]
    public void Two_different_single_mains_feed_two_people()
    {
        var combo = ComboSuggestion.Create(120m, 0.20m,
            [Dish(54.90m, ComboCourse.Main, 1), Dish(59.90m, ComboCourse.Main, 1)], people: 2);

        Assert.True(combo.IsSuccess);
    }

    [Fact]
    public void Course_with_unknown_portion_is_not_judged()
    {
        var combo = ComboSuggestion.Create(60m, 0.20m, [Dish(54.90m, ComboCourse.Main, null)], people: 2);

        Assert.True(combo.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Invalid_party_size_is_rejected(int people)
    {
        var combo = ComboSuggestion.Create(100m, 0.20m, [Line(100m)], people);

        Assert.Equal(ComboErrors.PeopleInvalid, combo.Error);
    }
}
