using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Combos;

public sealed record ComboLine(Guid ItemId, string ItemName, decimal UnitPrice, int Quantity)
{
    public decimal Subtotal => UnitPrice * Quantity;
}

/// <summary>
/// Set of menu items assembled for a customer budget. The total is always computed here from
/// the stored prices, never taken from the model. A combo is valid only when the total falls in
/// [budget × (1 − tolerance), budget × (1 + tolerance)].
/// </summary>
public sealed class ComboSuggestion
{
    public const int MaxQuantityPerLine = 20;
    public const int MaxLines = 15;

    private ComboSuggestion(decimal budget, decimal tolerance, IReadOnlyList<ComboLine> lines)
    {
        Budget = budget;
        Tolerance = tolerance;
        Lines = lines;
        Total = lines.Sum(l => l.Subtotal);
    }

    public decimal Budget { get; }
    public decimal Tolerance { get; }
    public IReadOnlyList<ComboLine> Lines { get; }
    public decimal Total { get; }
    public decimal MinAllowed => decimal.Round(Budget * (1 - Tolerance), 2);
    public decimal MaxAllowed => decimal.Round(Budget * (1 + Tolerance), 2);

    /// <summary>True when the total exceeds the budget; the customer must be told explicitly.</summary>
    public bool IsOverBudget => Total > Budget;

    public static Result<ComboSuggestion> Create(decimal budget, decimal tolerance, IReadOnlyList<ComboLine> lines)
    {
        if (budget <= 0)
            return ComboErrors.BudgetInvalid;
        if (tolerance < 0 || tolerance > 1)
            return ComboErrors.ToleranceInvalid;
        if (lines.Count == 0 || lines.Count > MaxLines)
            return ComboErrors.LinesInvalid;
        if (lines.Any(l => l.Quantity < 1 || l.Quantity > MaxQuantityPerLine || l.UnitPrice < 0))
            return ComboErrors.QuantityInvalid;
        if (lines.Select(l => l.ItemId).Distinct().Count() != lines.Count)
            return ComboErrors.DuplicatedItem;

        var combo = new ComboSuggestion(budget, tolerance, lines);

        if (combo.Total > combo.MaxAllowed)
            return ComboErrors.AboveRange;
        if (combo.Total < combo.MinAllowed)
            return ComboErrors.BelowRange;

        return combo;
    }
}

public static class ComboErrors
{
    public static readonly Error BudgetInvalid = Error.Validation("combo.budget_invalid");
    public static readonly Error ToleranceInvalid = Error.Validation("combo.tolerance_invalid");
    public static readonly Error LinesInvalid = Error.Validation("combo.lines_invalid");
    public static readonly Error QuantityInvalid = Error.Validation("combo.quantity_invalid");
    public static readonly Error DuplicatedItem = Error.Validation("combo.duplicated_item");
    public static readonly Error AboveRange = Error.Validation("combo.above_range");
    public static readonly Error BelowRange = Error.Validation("combo.below_range");
}
