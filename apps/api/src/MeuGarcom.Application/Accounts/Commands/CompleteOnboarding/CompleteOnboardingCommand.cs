using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Accounts.Commands.CompleteOnboarding;

public sealed record CompleteOnboardingCommand(
    string OwnerName,
    string RestaurantName,
    string? City,
    string? CuisineType,
    int TableCount) : ICommand<Result<OnboardingResult>>;

public sealed record OnboardingResult(Guid AccountId, Guid RestaurantId, Guid UnitId);
