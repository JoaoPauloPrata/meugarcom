using MeuGarcom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.IntegrationTests;

/// <summary>
/// EF orders INSERTs by the relationships it knows. A FK column without a mapped relationship
/// (e.g. Menu.UnitId) can be written before its parent and break the database constraint.
/// </summary>
public class PersistenceModelTests
{
    /// <summary>References to tables outside the app model (Supabase auth.users).</summary>
    private static readonly HashSet<string> External = ["Account.OwnerUserId"];

    [Fact]
    public void Every_reference_id_is_mapped_as_a_foreign_key()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var db = new AppDbContext(options);

        var unmapped = db.Model.GetEntityTypes()
            .SelectMany(entity => entity.GetProperties()
                .Where(p => p.Name != "Id" && p.Name.EndsWith("Id") && (p.ClrType == typeof(Guid) || p.ClrType == typeof(Guid?)))
                .Where(p => !p.IsForeignKey())
                .Select(p => $"{entity.ClrType.Name}.{p.Name}"))
            .Where(name => !External.Contains(name))
            .ToList();

        Assert.Empty(unmapped);
    }
}
