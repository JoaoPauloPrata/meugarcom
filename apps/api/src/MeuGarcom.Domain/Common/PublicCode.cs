using System.Security.Cryptography;

namespace MeuGarcom.Domain.Common;

/// <summary>Unguessable short code used in public links (QR code / NFC).</summary>
public static class PublicCode
{
    private const string Alphabet = "abcdefghjkmnpqrstuvwxyz23456789";
    public const int Length = 10;

    public static string New() => RandomNumberGenerator.GetString(Alphabet, Length);

    public static bool IsWellFormed(string? code) =>
        code is { Length: >= 6 and <= 16 } && code.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9');
}
