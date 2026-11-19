using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Accounts;

/// <summary>Physical location and billing unit. Has its own menu and dining tables.</summary>
public sealed class Unit : Entity
{
    public const int MaxNameLength = 120;
    public const int MaxCityLength = 120;
    public const int MaxTables = 300;
    public const decimal DefaultComboBudgetTolerance = 0.20m;
    public const decimal MaxComboBudgetTolerance = 0.50m;

    private readonly List<DiningTable> _tables = [];

    private Unit() { }

    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? City { get; private set; }
    public string PublicCode { get; private set; } = null!;

    /// <summary>Allowed distance from the customer's budget when assembling a combo (0.20 = ±20%).</summary>
    public decimal ComboBudgetTolerance { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<DiningTable> Tables => _tables.AsReadOnly();

    internal static Result<Unit> Create(Guid restaurantId, string name, string? city, DateTimeOffset now)
    {
        var trimmed = name?.Trim() ?? "";
        if (trimmed.Length is 0 or > MaxNameLength)
            return UnitErrors.NameInvalid;

        var trimmedCity = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        if (trimmedCity is { Length: > MaxCityLength })
            return UnitErrors.CityInvalid;

        return new Unit
        {
            Id = Guid.CreateVersion7(),
            RestaurantId = restaurantId,
            Name = trimmed,
            City = trimmedCity,
            PublicCode = Common.PublicCode.New(),
            ComboBudgetTolerance = DefaultComboBudgetTolerance,
            CreatedAt = now,
        };
    }

    public Result UpdateDetails(string name, string? city)
    {
        var trimmed = name?.Trim() ?? "";
        if (trimmed.Length is 0 or > MaxNameLength)
            return UnitErrors.NameInvalid;

        var trimmedCity = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        if (trimmedCity is { Length: > MaxCityLength })
            return UnitErrors.CityInvalid;

        Name = trimmed;
        City = trimmedCity;
        return Result.Success();
    }

    public Result ChangeComboBudgetTolerance(decimal tolerance)
    {
        if (tolerance < 0 || tolerance > MaxComboBudgetTolerance)
            return UnitErrors.ComboToleranceOutOfRange;

        ComboBudgetTolerance = decimal.Round(tolerance, 2);
        return Result.Success();
    }

    /// <summary>Adds <paramref name="count"/> tables numbered after the highest existing number.</summary>
    public Result<IReadOnlyList<DiningTable>> AddTables(int count, DateTimeOffset now)
    {
        if (count < 1)
            return UnitErrors.TableCountInvalid;
        if (_tables.Count + count > MaxTables)
            return UnitErrors.TooManyTables;

        var next = _tables.Count == 0 ? 1 : _tables.Max(t => t.Number) + 1;
        var added = new List<DiningTable>(count);
        for (var i = 0; i < count; i++)
        {
            var table = DiningTable.Create(Id, next + i, now);
            _tables.Add(table);
            added.Add(table);
        }

        return added;
    }

    public Result RemoveTable(Guid tableId)
    {
        var table = _tables.FirstOrDefault(t => t.Id == tableId);
        if (table is null)
            return UnitErrors.TableNotFound;

        _tables.Remove(table);
        return Result.Success();
    }
}
