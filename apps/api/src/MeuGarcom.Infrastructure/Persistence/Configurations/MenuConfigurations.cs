using System.Text.Json;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Menus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuGarcom.Infrastructure.Persistence.Configurations;

internal sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("menus");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.UnitId).IsUnique();
        builder.HasOne<Unit>().WithOne().HasForeignKey<Menu>(m => m.UnitId);
        builder.HasMany(m => m.Categories).WithOne().HasForeignKey(c => c.MenuId);
        builder.HasMany(m => m.Items).WithOne().HasForeignKey(i => i.MenuId);
        builder.Navigation(m => m.Categories).HasField("_categories");
        builder.Navigation(m => m.Items).HasField("_items");
    }
}

internal sealed class MenuCategoryConfiguration : IEntityTypeConfiguration<MenuCategory>
{
    public void Configure(EntityTypeBuilder<MenuCategory> builder)
    {
        builder.ToTable("menu_categories");
        builder.HasKey(c => c.Id);
        builder.HasMany<MenuItem>().WithOne().HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Price).HasPrecision(10, 2);
        builder.Property(i => i.ServesPeople).HasColumnType("smallint");
        builder.Property(i => i.ImagePath).HasMaxLength(MenuItem.MaxImagePathLength);
        builder.Ignore(i => i.Dietary);
    }
}

internal sealed class MenuImportConfiguration : IEntityTypeConfiguration<MenuImport>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<MenuImport> builder)
    {
        builder.ToTable("menu_imports");
        builder.HasKey(i => i.Id);
        builder.HasOne<Unit>().WithMany().HasForeignKey(i => i.UnitId);
        builder.Property(i => i.Status).HasConversion(EnumText<MenuImportStatus>.Converter);
        builder.Property(i => i.Draft)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<ImportedMenuItem>>(v, JsonOptions) ?? new List<ImportedMenuItem>(),
                new ValueComparer<IReadOnlyList<ImportedMenuItem>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    v => v.ToList()));
    }
}
