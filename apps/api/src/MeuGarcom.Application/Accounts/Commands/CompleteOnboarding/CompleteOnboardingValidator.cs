using FluentValidation;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Domain.Accounts;

namespace MeuGarcom.Application.Accounts.Commands.CompleteOnboarding;

public sealed class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.OwnerName).NotEmpty().WithMessage(UserMessages.Required)
            .MaximumLength(Account.MaxNameLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.RestaurantName).NotEmpty().WithMessage(UserMessages.Required)
            .MaximumLength(Restaurant.MaxNameLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.City).MaximumLength(Unit.MaxCityLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.CuisineType).MaximumLength(Restaurant.MaxCuisineTypeLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.TableCount).InclusiveBetween(0, Unit.MaxTables).WithMessage(UserMessages.InvalidValue);
    }
}
