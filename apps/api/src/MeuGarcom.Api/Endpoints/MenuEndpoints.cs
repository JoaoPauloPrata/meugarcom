using Mediator;
using MeuGarcom.Api.Http;
using MeuGarcom.Application.Menus.Commands;
using MeuGarcom.Application.Menus.Commands.CreateCategory;
using MeuGarcom.Application.Menus.Commands.CreateMenuItem;
using MeuGarcom.Application.Menus.Commands.DeleteCategory;
using MeuGarcom.Application.Menus.Commands.DeleteMenuItem;
using MeuGarcom.Application.Menus.Commands.ImportMenu;
using MeuGarcom.Application.Menus.Commands.PublishMenuImport;
using MeuGarcom.Application.Menus.Commands.RemoveMenuItemImage;
using MeuGarcom.Application.Menus.Commands.RenameCategory;
using MeuGarcom.Application.Menus.Commands.SetMenuItemAvailability;
using MeuGarcom.Application.Menus.Commands.SetMenuItemImage;
using MeuGarcom.Application.Menus.Commands.UpdateMenuItem;
using MeuGarcom.Application.Menus.Queries.GetMenu;

namespace MeuGarcom.Api.Endpoints;

public sealed class MenuEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/units/{unitId:guid}/menu").WithTags("Menu").RequireAuthorization();

        group.MapGet("/", async (Guid unitId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetMenuQuery(unitId), ct)).ToHttp());

        group.MapPost("/categories", async (Guid unitId, NameRequest body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateCategoryCommand(unitId, body.Name), ct)).ToHttp());

        group.MapPut("/categories/{categoryId:guid}", async (Guid unitId, Guid categoryId, NameRequest body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RenameCategoryCommand(unitId, categoryId, body.Name), ct)).ToHttp());

        group.MapDelete("/categories/{categoryId:guid}", async (Guid unitId, Guid categoryId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteCategoryCommand(unitId, categoryId), ct)).ToHttp());

        group.MapPost("/items", async (Guid unitId, MenuItemInput body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateMenuItemCommand(unitId, body), ct)).ToHttp());

        group.MapPut("/items/{itemId:guid}", async (Guid unitId, Guid itemId, MenuItemInput body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateMenuItemCommand(unitId, itemId, body), ct)).ToHttp());

        group.MapPatch("/items/{itemId:guid}/availability", async (Guid unitId, Guid itemId, AvailabilityRequest body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SetMenuItemAvailabilityCommand(unitId, itemId, body.IsAvailable), ct)).ToHttp());

        group.MapDelete("/items/{itemId:guid}", async (Guid unitId, Guid itemId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteMenuItemCommand(unitId, itemId), ct)).ToHttp());

        // Bearer-token API (no cookies), so antiforgery does not apply to the uploads.
        group.MapPut("/items/{itemId:guid}/image", async (Guid unitId, Guid itemId, IFormFile file, ISender sender, CancellationToken ct) =>
            {
                var command = new SetMenuItemImageCommand(unitId, itemId, ResolveContentType(file), await ReadAllAsync(file, ct));
                return (await sender.Send(command, ct)).ToHttp();
            })
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttributeMetadata(SetMenuItemImageValidator.MaxFileBytes + 64 * 1024));

        group.MapDelete("/items/{itemId:guid}/image", async (Guid unitId, Guid itemId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RemoveMenuItemImageCommand(unitId, itemId), ct)).ToHttp());

        group.MapPost("/imports", async (Guid unitId, IFormFile file, ISender sender, CancellationToken ct) =>
            {
                var command = new ImportMenuCommand(unitId, file.FileName, ResolveContentType(file), await ReadAllAsync(file, ct));
                return (await sender.Send(command, ct)).ToHttp();
            })
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttributeMetadata(ImportMenuValidator.MaxFileBytes + 64 * 1024));

        group.MapPost("/imports/{importId:guid}/publish", async (Guid unitId, Guid importId, PublishRequest body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new PublishMenuImportCommand(unitId, importId, body.Items), ct)).ToHttp());
    }

    private static async Task<byte[]> ReadAllAsync(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        return buffer.ToArray();
    }

    /// <summary>Browsers often send spreadsheets as application/octet-stream; trust the extension then.</summary>
    private static string ResolveContentType(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".csv" => "text/csv",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => file.ContentType,
        };
    }

    private sealed record NameRequest(string Name);

    private sealed record AvailabilityRequest(bool IsAvailable);

    private sealed record PublishRequest(IReadOnlyList<ReviewedItem> Items);

    private sealed class RequestSizeLimitAttributeMetadata(long limit) : Microsoft.AspNetCore.Http.Metadata.IRequestSizeLimitMetadata
    {
        public long? MaxRequestBodySize => limit;
    }
}
