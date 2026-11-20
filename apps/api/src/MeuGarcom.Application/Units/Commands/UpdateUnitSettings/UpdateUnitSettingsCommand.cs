using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Units.Commands.UpdateUnitSettings;

/// <param name="ComboBudgetTolerancePercent">0 to 50 (20 = ±20%).</param>
public sealed record UpdateUnitSettingsCommand(
    Guid UnitId,
    string RestaurantName,
    string? CuisineType,
    string UnitName,
    string? City,
    int ComboBudgetTolerancePercent,
    IReadOnlyList<ChatSuggestionInput> ChatSuggestions) : ICommand<Result>;

public sealed record ChatSuggestionInput(string Text, string? Icon);
