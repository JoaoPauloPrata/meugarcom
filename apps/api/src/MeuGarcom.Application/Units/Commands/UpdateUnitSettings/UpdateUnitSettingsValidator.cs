using FluentValidation;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Domain.Accounts;

namespace MeuGarcom.Application.Units.Commands.UpdateUnitSettings;

public sealed class UpdateUnitSettingsValidator : AbstractValidator<UpdateUnitSettingsCommand>
{
    public UpdateUnitSettingsValidator()
    {
        RuleFor(x => x.RestaurantName).NotEmpty().WithMessage(UserMessages.Required)
            .MaximumLength(Restaurant.MaxNameLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.CuisineType).MaximumLength(Restaurant.MaxCuisineTypeLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.UnitName).NotEmpty().WithMessage(UserMessages.Required)
            .MaximumLength(Unit.MaxNameLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.City).MaximumLength(Unit.MaxCityLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.ComboBudgetTolerancePercent).InclusiveBetween(0, 50).WithMessage(UserMessages.InvalidValue);
    }
}
