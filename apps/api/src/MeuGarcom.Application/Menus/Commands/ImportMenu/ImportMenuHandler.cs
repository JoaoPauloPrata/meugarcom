using System.Text.Json;
using System.Text.Json.Serialization;
using Mediator;
using MeuGarcom.Application.Common.Guardrails;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Persistence;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeuGarcom.Application.Menus.Commands.ImportMenu;

public sealed class ImportMenuHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IWaiterAssistant assistant,
    TimeProvider clock,
    ILogger<ImportMenuHandler> logger) : ICommandHandler<ImportMenuCommand, Result<MenuImportDto>>
{
    public static readonly Error ExtractionFailed = Error.Failure("menu.import_failed");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<Result<MenuImportDto>> Handle(ImportMenuCommand command, CancellationToken ct)
    {
        var unit = await db.UnitsOwnedBy(currentUser.UserId)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == command.UnitId, ct);
        if (unit is null)
            return UnitErrors.NotFound;

        ExtractionOutput? output;
        try
        {
            var completion = await assistant.CompleteAsync(new AiCompletionRequest(
                AiTask.MenuExtraction,
                unit.RestaurantId,
                MenuExtractionPrompt.Instructions,
                ContextData: null,
                [new AiMessage(AiRole.User, MenuExtractionPrompt.UserRequest)],
                MenuExtractionPrompt.SchemaName,
                MenuExtractionPrompt.Schema,
                new AiAttachment(command.FileName, command.ContentType, command.Content)), ct);

            output = JsonSerializer.Deserialize<ExtractionOutput>(completion.Json, JsonOptions);
        }
        catch (Exception ex) when (ex is AiUnavailableException or JsonException)
        {
            logger.LogWarning(ex, "Menu extraction failed for unit {UnitId}", command.UnitId);
            return ExtractionFailed;
        }

        var items = (output?.Items ?? [])
            .Where(i => !string.IsNullOrWhiteSpace(i.Name))
            .Take(Menu.MaxItems)
            .Select(ToImported)
            .ToList();
        if (items.Count == 0)
            return ExtractionFailed;

        var import = MenuImport.Create(unit.Id, command.FileName, items.Select(i => i.Item).ToList(), clock.GetUtcNow());
        db.MenuImports.Add(import);
        await db.SaveChangesAsync(ct);

        return new MenuImportDto(
            import.Id,
            import.FileName,
            items.Select(i => new ImportedItemDto(
                i.Item.CategoryName, i.Item.Name, i.Item.Description, i.Item.Ingredients, i.Item.Price,
                i.Item.ServesPeople, i.Item.Portion, i.Item.IsSpicy, i.NeedsReview)).ToList());
    }

    private static (ImportedMenuItem Item, bool NeedsReview) ToImported(ExtractedItem extracted)
    {
        static string? Clip(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var trimmed = value.Trim();
            return trimmed.Length > max ? trimmed[..max] : trimmed;
        }

        var price = extracted.Price is > 0 and <= (double)MenuItemDetails.MaxPrice
            ? decimal.Round((decimal)extracted.Price.Value, 2)
            : 0m;
        var serves = extracted.ServesPeople is >= 1 and <= 50 ? extracted.ServesPeople : null;

        var item = new ImportedMenuItem(
            Clip(extracted.Category, 80) ?? "Cardápio",
            Clip(extracted.Name, MenuItemDetails.MaxNameLength)!,
            Clip(extracted.Description, MenuItemDetails.MaxTextLength),
            Clip(extracted.Ingredients, MenuItemDetails.MaxTextLength),
            price,
            serves,
            Clip(extracted.Portion, MenuItemDetails.MaxPortionLength),
            extracted.IsSpicy);

        // Indirect prompt injection: text extracted from a file must never reach the chat unreviewed.
        var suspicious = InjectionPatterns.IsSuspicious(item.Name)
            || InjectionPatterns.IsSuspicious(item.Description)
            || InjectionPatterns.IsSuspicious(item.Ingredients)
            || InjectionPatterns.IsSuspicious(item.CategoryName);

        return (item, suspicious || price == 0m);
    }

    private sealed record ExtractionOutput([property: JsonPropertyName("items")] List<ExtractedItem>? Items);

    private sealed record ExtractedItem(
        string? Category,
        string? Name,
        string? Description,
        string? Ingredients,
        double? Price,
        int? ServesPeople,
        string? Portion,
        bool IsSpicy);
}
