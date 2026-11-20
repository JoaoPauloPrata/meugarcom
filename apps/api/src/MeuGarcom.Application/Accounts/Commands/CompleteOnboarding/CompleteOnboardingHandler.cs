using Mediator;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Menus;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Accounts.Commands.CompleteOnboarding;

/// <summary>Creates account → restaurant → first unit (free plan) → empty menu → tables.</summary>
public sealed class CompleteOnboardingHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<CompleteOnboardingCommand, Result<OnboardingResult>>
{
    public async ValueTask<Result<OnboardingResult>> Handle(CompleteOnboardingCommand command, CancellationToken ct)
    {
        if (await db.Accounts.AnyAsync(a => a.OwnerUserId == currentUser.UserId, ct))
            return AccountErrors.AlreadyExists;

        var now = clock.GetUtcNow();

        var account = Account.Create(currentUser.UserId, command.OwnerName, now);
        if (account.IsFailure)
            return account.Error!;

        var restaurant = account.Value.AddRestaurant(command.RestaurantName, command.CuisineType, now);
        if (restaurant.IsFailure)
            return restaurant.Error!;

        var unitName = string.IsNullOrWhiteSpace(command.City) ? restaurant.Value.Name : command.City;
        var unit = restaurant.Value.AddUnit(unitName, command.City, now);
        if (unit.IsFailure)
            return unit.Error!;

        unit.Value.ChangeChatSuggestions(DefaultChatSuggestions.All);

        if (command.TableCount > 0)
        {
            var tables = unit.Value.AddTables(command.TableCount, now);
            if (tables.IsFailure)
                return tables.Error!;
        }

        db.Accounts.Add(account.Value);
        db.Menus.Add(Menu.CreateFor(unit.Value.Id, now));
        await db.SaveChangesAsync(ct);

        return new OnboardingResult(account.Value.Id, restaurant.Value.Id, unit.Value.Id);
    }
}
