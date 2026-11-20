using System.Text.Json;
using MeuGarcom.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuGarcom.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.OwnerUserId).IsUnique();
        builder.HasMany(a => a.Restaurants).WithOne().HasForeignKey(r => r.AccountId);
        builder.Navigation(a => a.Restaurants).HasField("_restaurants");
    }
}

internal sealed class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("restaurants");
        builder.HasKey(r => r.Id);
        builder.HasMany(r => r.Units).WithOne().HasForeignKey(u => u.RestaurantId);
        builder.Navigation(r => r.Units).HasField("_units");
    }
}

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("units");
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.PublicCode).IsUnique();
        builder.Property(u => u.ComboBudgetTolerance).HasPrecision(3, 2);
        builder.Property(u => u.ChatSuggestions)
            .HasField("_chatSuggestions")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnType("jsonb")
            .HasConversion(
                suggestions => JsonSerializer.Serialize(suggestions, JsonOptions),
                json => JsonSerializer.Deserialize<List<ChatSuggestion>>(json, JsonOptions) ?? new List<ChatSuggestion>(),
                new ValueComparer<IReadOnlyList<ChatSuggestion>>(
                    (a, b) => a!.SequenceEqual(b!),
                    list => list.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                    list => list.ToList()));
        builder.HasMany(u => u.Tables).WithOne().HasForeignKey(t => t.UnitId);
        builder.Navigation(u => u.Tables).HasField("_tables");
    }
}

internal sealed class DiningTableConfiguration : IEntityTypeConfiguration<DiningTable>
{
    public void Configure(EntityTypeBuilder<DiningTable> builder)
    {
        builder.ToTable("dining_tables");
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.PublicCode).IsUnique();
    }
}
