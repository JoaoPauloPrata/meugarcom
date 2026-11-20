using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Combos;

/// <summary>Part of the meal a combo line belongs to; party size is checked per course.</summary>
public enum ComboCourse
{
    Starter,
    Main,
    Side,
    Dessert,
    Drink,
}

/// <param name="ServesPeople">Portion informed in the menu; null when the menu does not say.</param>
public sealed record ComboLine(
    Guid ItemId,
    string ItemName,
    decimal UnitPrice,
    int Quantity,
    ComboCourse Course = ComboCourse.Main,
    int? ServesPeople = null)
{
    public decimal Subtotal => UnitPrice * Quantity;

    /// <summary>People this line feeds, or null when the menu does not inform the portion.</summary>
    public int? PeopleServed => ServesPeople * Quantity;
}

/// <summary>
/// Set of menu items assembled for a customer budget. The total is always computed here from
/// the stored prices, never taken from the model. A combo is valid only when the total falls in
/// [budget × (1 − tolerance), budget × (1 + tolerance)] and, when the party size is known, every
/// course whose portions are informed feeds the whole party.
/// </summary>
public sealed class ComboSuggestion
{
    public const int MaxQuantityPerLine = 20;
    public const int MaxLines = 15;

    public const int MaxPeople = 50;

    private ComboSuggestion(decimal budget, decimal tolerance, int? people, IReadOnlyList<ComboLine> lines)
    {
        Budget = budget;
        Tolerance = tolerance;
        People = people;
        Lines = lines;
        Total = lines.Sum(l => l.Subtotal);
    }

    public decimal Budget { get; }
    public decimal Tolerance { get; }

    /// <summary>Party size the customer asked for; null when not informed.</summary>
    public int? People { get; }
    public IReadOnlyList<ComboLine> Lines { get; }
    public decimal Total { get; }
    public decimal MinAllowed => decimal.Round(Budget * (1 - Tolerance), 2);
    public decimal MaxAllowed => decimal.Round(Budget * (1 + Tolerance), 2);

    /// <summary>True when the total exceeds the budget; the customer must be told explicitly.</summary>
    public bool IsOverBudget => Total > Budget;

    /// <summary>
    /// Courses whose informed portions do not feed the party. A course with any line of unknown
    /// portion is skipped: the menu does not inform it, so it can't be judged.
    /// </summary>
    public static IReadOnlyList<ComboCourse> UnderservedCourses(int? people, IReadOnlyList<ComboLine> lines) =>
        people is not { } party
            ? []
            : lines.GroupBy(l => l.Course)
                .Where(g => g.All(l => l.PeopleServed is not null) && g.Sum(l => l.PeopleServed!.Value) < party)
                .Select(g => g.Key)
                .ToList();

    public static Result<ComboSuggestion> Create(
        decimal budget, decimal tolerance, IReadOnlyList<ComboLine> lines, int? people = null)
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
        if (people is < 1 or > MaxPeople)
            return ComboErrors.PeopleInvalid;
        if (UnderservedCourses(people, lines).Count > 0)
            return ComboErrors.NotEnoughForParty;

        var combo = new ComboSuggestion(budget, tolerance, people, lines);

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
    public static readonly Error PeopleInvalid = Error.Validation("combo.people_invalid");
    public static readonly Error NotEnoughForParty = Error.Validation("combo.not_enough_for_party");
    public static readonly Error AboveRange = Error.Validation("combo.above_range");
    public static readonly Error BelowRange = Error.Validation("combo.below_range");
}
