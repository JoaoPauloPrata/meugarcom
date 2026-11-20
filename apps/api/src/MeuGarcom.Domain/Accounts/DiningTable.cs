using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Accounts;

/// <summary>Physical access point (QR code / NFC tag) that tells where the customer is sitting.</summary>
public sealed class DiningTable : Entity
{
    private DiningTable() { }

    public Guid UnitId { get; private set; }
    public int Number { get; private set; }
    public string PublicCode { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    internal static DiningTable Create(Guid unitId, int number, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        UnitId = unitId,
        Number = number,
        PublicCode = Common.PublicCode.New(),
        CreatedAt = now,
    };
}
