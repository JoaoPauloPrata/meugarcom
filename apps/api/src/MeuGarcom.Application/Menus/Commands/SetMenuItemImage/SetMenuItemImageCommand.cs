using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Menus.Commands.SetMenuItemImage;

/// <summary>Uploads (or replaces) the item photo. Returns the public URL of the new photo.</summary>
public sealed record SetMenuItemImageCommand(Guid UnitId, Guid ItemId, string ContentType, byte[] Content)
    : ICommand<Result<string>>;
