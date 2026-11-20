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
        RuleFor(x => x.ChatSuggestions).NotNull().WithMessage(UserMessages.InvalidValue)
            .Must(s => s is null || s.Count <= Unit.MaxChatSuggestions).WithMessage(UserMessages.ForCode(UnitErrors.TooManyChatSuggestions.Code));
        RuleForEach(x => x.ChatSuggestions).ChildRules(suggestion =>
        {
            suggestion.RuleFor(s => s.Text).NotEmpty().WithMessage(UserMessages.Required)
                .MaximumLength(ChatSuggestion.MaxTextLength).WithMessage(UserMessages.TooLong);
            suggestion.RuleFor(s => s.Icon).Must(i => i is null || ChatSuggestion.Icons.Contains(i)).WithMessage(UserMessages.InvalidValue);
        });
    }
}
