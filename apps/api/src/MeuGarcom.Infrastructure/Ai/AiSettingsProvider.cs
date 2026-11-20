using MeuGarcom.Domain.Ai;
using MeuGarcom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MeuGarcom.Infrastructure.Ai;

/// <summary>
/// Reads <see cref="AiModelSettings"/> from the database: per-restaurant override first, then the
/// global default. Cached for a minute, so a model change takes effect without a deploy.
/// </summary>
internal sealed class AiSettingsProvider(AppDbContext db, IMemoryCache cache)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

    public async Task<AiModelSettings> GetAsync(Guid? restaurantId, CancellationToken ct)
    {
        var key = $"ai-settings:{restaurantId?.ToString() ?? "global"}";
        if (cache.TryGetValue(key, out AiModelSettings? cached) && cached is not null)
            return cached;

        var settings = await db.AiModelSettings.AsNoTracking()
            .Where(s => s.RestaurantId == restaurantId || s.RestaurantId == null)
            .OrderBy(s => s.RestaurantId == null) // the restaurant override wins
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No AI model settings found in the database (ai_model_settings).");

        cache.Set(key, settings, CacheDuration);
        return settings;
    }
}
