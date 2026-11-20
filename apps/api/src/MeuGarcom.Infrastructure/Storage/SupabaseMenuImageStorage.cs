using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeuGarcom.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeuGarcom.Infrastructure.Storage;

public sealed record StorageOptions(string SupabaseUrl, string? ServiceKey)
{
    public const string MenuImagesBucket = "menu-images";
}

/// <summary>Menu photos in a public Supabase Storage bucket, written through the Storage REST API.</summary>
internal sealed class SupabaseMenuImageStorage(
    HttpClient http,
    StorageOptions options,
    ILogger<SupabaseMenuImageStorage> logger) : IMenuImageStorage
{
    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = "jpg",
        ["image/png"] = "png",
        ["image/webp"] = "webp",
    };

    public async Task<string> UploadAsync(Guid unitId, Guid itemId, byte[] content, string contentType, CancellationToken ct)
    {
        var extension = Extensions.GetValueOrDefault(contentType)
            ?? throw new ArgumentException($"Unsupported image type: {contentType}", nameof(contentType));
        var path = $"{unitId:N}/{itemId:N}-{Guid.NewGuid():N}.{extension}";

        using var request = CreateRequest(HttpMethod.Post, $"storage/v1/object/{StorageOptions.MenuImagesBucket}/{path}");
        request.Content = new ByteArrayContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        // Every upload gets a new path, so the object never changes and can be cached for a year.
        request.Headers.TryAddWithoutValidation("cache-control", "max-age=31536000");
        request.Headers.Add("x-upsert", "false");

        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Menu image upload failed ({(int)response.StatusCode}): {body}");
        }

        return path;
    }

    public async Task DeleteAsync(IReadOnlyCollection<string> paths, CancellationToken ct)
    {
        if (paths.Count == 0)
            return;

        try
        {
            using var request = CreateRequest(HttpMethod.Delete, $"storage/v1/object/{StorageOptions.MenuImagesBucket}");
            request.Content = JsonContent.Create(new { prefixes = paths });
            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                logger.LogWarning("Menu image delete failed with status {Status} for {Count} file(s)", (int)response.StatusCode, paths.Count);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Menu image delete failed for {Count} file(s)", paths.Count);
        }
    }

    public string GetPublicUrl(string path) =>
        $"{options.SupabaseUrl.TrimEnd('/')}/storage/v1/object/public/{StorageOptions.MenuImagesBucket}/{path}";

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceKey))
            throw new InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY is not configured.");

        var request = new HttpRequestMessage(method, $"{options.SupabaseUrl.TrimEnd('/')}/{relativeUrl}");
        request.Headers.Add("apikey", options.ServiceKey);
        // Legacy service_role keys are JWTs and go in Authorization too; new sb_secret_ keys only in apikey.
        if (options.ServiceKey.StartsWith("eyJ", StringComparison.Ordinal))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ServiceKey);
        return request;
    }
}
