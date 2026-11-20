namespace MeuGarcom.Application.Common.Interfaces;

/// <summary>Public file storage for menu item photos. Paths are stored on the item, URLs are derived.</summary>
public interface IMenuImageStorage
{
    /// <summary>Stores the photo under a new unique path (so caches never serve a stale image) and returns it.</summary>
    Task<string> UploadAsync(Guid unitId, Guid itemId, byte[] content, string contentType, CancellationToken ct);

    /// <summary>Best effort: failures are logged, never thrown, since the database is already consistent.</summary>
    Task DeleteAsync(IReadOnlyCollection<string> paths, CancellationToken ct);

    string GetPublicUrl(string path);
}
