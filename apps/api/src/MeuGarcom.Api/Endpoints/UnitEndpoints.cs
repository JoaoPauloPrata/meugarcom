using Mediator;
using MeuGarcom.Api.Http;
using MeuGarcom.Application.Conversations.Queries.ListConversations;
using MeuGarcom.Application.Tables.Commands.AddTables;
using MeuGarcom.Application.Tables.Commands.RemoveTable;
using MeuGarcom.Application.Tables.Queries.ListTables;
using MeuGarcom.Application.Units.Commands.UpdateUnitSettings;
using MeuGarcom.Application.Units.Queries.GetUnitOverview;

namespace MeuGarcom.Api.Endpoints;

public sealed class UnitEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/units/{unitId:guid}").WithTags("Units").RequireAuthorization();

        group.MapGet("/overview", async (Guid unitId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetUnitOverviewQuery(unitId), ct)).ToHttp());

        group.MapPut("/settings", async (Guid unitId, UnitSettingsRequest body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateUnitSettingsCommand(
                unitId, body.RestaurantName, body.CuisineType, body.UnitName, body.City, body.ComboBudgetTolerancePercent,
                body.ChatSuggestions ?? []), ct)).ToHttp());

        group.MapGet("/tables", async (Guid unitId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListTablesQuery(unitId), ct)).ToHttp());

        group.MapPost("/tables", async (Guid unitId, AddTablesRequest body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AddTablesCommand(unitId, body.Count), ct)).ToHttp());

        group.MapDelete("/tables/{tableId:guid}", async (Guid unitId, Guid tableId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RemoveTableCommand(unitId, tableId), ct)).ToHttp());

        group.MapGet("/conversations", async (Guid unitId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListConversationsQuery(unitId), ct)).ToHttp());
    }

    private sealed record UnitSettingsRequest(
        string RestaurantName,
        string? CuisineType,
        string UnitName,
        string? City,
        int ComboBudgetTolerancePercent,
        IReadOnlyList<ChatSuggestionInput>? ChatSuggestions);

    private sealed record AddTablesRequest(int Count);
}
