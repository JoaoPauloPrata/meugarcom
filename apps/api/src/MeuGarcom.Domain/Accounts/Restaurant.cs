using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Accounts;

/// <summary>Brand/establishment inside an account. Owns units.</summary>
public sealed class Restaurant : Entity
{
    public const int MaxNameLength = 160;
    public const int MaxCuisineTypeLength = 60;

    private readonly List<Unit> _units = [];

    private Restaurant() { }

    public Guid AccountId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? CuisineType { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<Unit> Units => _units.AsReadOnly();

    internal static Result<Restaurant> Create(Guid accountId, string name, string? cuisineType, DateTimeOffset now)
    {
        var trimmed = name?.Trim() ?? "";
        if (trimmed.Length is 0 or > MaxNameLength)
            return RestaurantErrors.NameInvalid;

        var cuisine = string.IsNullOrWhiteSpace(cuisineType) ? null : cuisineType.Trim();
        if (cuisine is { Length: > MaxCuisineTypeLength })
            return RestaurantErrors.CuisineTypeInvalid;

        return new Restaurant
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Name = trimmed,
            CuisineType = cuisine,
            CreatedAt = now,
        };
    }

    public Result<Unit> AddUnit(string name, string? city, DateTimeOffset now)
    {
        var unit = Unit.Create(Id, name, city, now);
        if (unit.IsFailure)
            return unit.Error!;

        _units.Add(unit.Value);
        return unit.Value;
    }

    public Result Rename(string name, string? cuisineType)
    {
        var trimmed = name?.Trim() ?? "";
        if (trimmed.Length is 0 or > MaxNameLength)
            return RestaurantErrors.NameInvalid;

        var cuisine = string.IsNullOrWhiteSpace(cuisineType) ? null : cuisineType.Trim();
        if (cuisine is { Length: > MaxCuisineTypeLength })
            return RestaurantErrors.CuisineTypeInvalid;

        Name = trimmed;
        CuisineType = cuisine;
        return Result.Success();
    }
}
