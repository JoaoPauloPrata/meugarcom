using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Accounts;

/// <summary>Paying customer (owner or chain). Owns restaurants. One account per auth user.</summary>
public sealed class Account : Entity
{
    public const int MaxNameLength = 120;

    private readonly List<Restaurant> _restaurants = [];

    private Account() { }

    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<Restaurant> Restaurants => _restaurants.AsReadOnly();

    public static Result<Account> Create(Guid ownerUserId, string name, DateTimeOffset now)
    {
        if (ownerUserId == Guid.Empty)
            return AccountErrors.OwnerRequired;

        var trimmed = name?.Trim() ?? "";
        if (trimmed.Length is 0 or > MaxNameLength)
            return AccountErrors.NameInvalid;

        return new Account
        {
            Id = Guid.CreateVersion7(),
            OwnerUserId = ownerUserId,
            Name = trimmed,
            CreatedAt = now,
        };
    }

    public Result<Restaurant> AddRestaurant(string name, string? cuisineType, DateTimeOffset now)
    {
        var restaurant = Restaurant.Create(Id, name, cuisineType, now);
        if (restaurant.IsFailure)
            return restaurant.Error!;

        _restaurants.Add(restaurant.Value);
        return restaurant.Value;
    }
}
