using MeuGarcom.Application.Common.Interfaces;

namespace MeuGarcom.Api.Http;

/// <summary>Reads the Supabase Auth user from the validated JWT ("sub" claim).</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirst("sub")?.Value, out var id)
            ? id
            : throw new UnauthorizedAccessException("No authenticated user in the current request.");

    public string? Email => accessor.HttpContext?.User.FindFirst("email")?.Value;
}
