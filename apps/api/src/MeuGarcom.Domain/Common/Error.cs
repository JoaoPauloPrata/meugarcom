namespace MeuGarcom.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Failure,
}

/// <summary>
/// A domain error. <see cref="Code"/> is a stable machine-readable key; the text shown to the
/// end user comes from a message catalog keyed by this code, never from the domain.
/// </summary>
public sealed record Error(string Code, ErrorType Type)
{
    public static Error Validation(string code) => new(code, ErrorType.Validation);
    public static Error NotFound(string code) => new(code, ErrorType.NotFound);
    public static Error Conflict(string code) => new(code, ErrorType.Conflict);
    public static Error Forbidden(string code) => new(code, ErrorType.Forbidden);
    public static Error Failure(string code) => new(code, ErrorType.Failure);
}
