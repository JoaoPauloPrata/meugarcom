using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.SetMenuItemImage;

public sealed class SetMenuItemImageHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IMenuImageStorage storage,
    TimeProvider clock) : ICommandHandler<SetMenuItemImageCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(SetMenuItemImageCommand command, CancellationToken ct)
    {
        var menu = await db.LoadOwnedMenuAsync(currentUser.UserId, command.UnitId, ct);
        if (menu is null)
            return MenuErrors.MenuNotFound;
        if (menu.Items.All(i => i.Id != command.ItemId))
            return MenuErrors.ItemNotFound;

        var path = await storage.UploadAsync(command.UnitId, command.ItemId, command.Content, command.ContentType, ct);

        var set = menu.SetItemImage(command.ItemId, path, clock.GetUtcNow());
        if (set.IsFailure)
        {
            await storage.DeleteAsync([path], CancellationToken.None);
            return set.Error!;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync([path], CancellationToken.None);
            throw;
        }

        if (set.Value is { } previous)
            await storage.DeleteAsync([previous], CancellationToken.None);

        return storage.GetPublicUrl(path);
    }
}
