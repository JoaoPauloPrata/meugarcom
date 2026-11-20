namespace MeuGarcom.Application.Common.Interfaces;

/// <summary>Authenticated restaurant owner (Supabase Auth user) of the current request.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    string? Email { get; }
}
