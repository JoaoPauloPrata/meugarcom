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
}
