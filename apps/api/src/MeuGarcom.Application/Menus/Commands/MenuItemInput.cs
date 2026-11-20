using FluentValidation;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands;

/// <summary>Item fields typed by a person in the menu settings (includes the manual dietary flags).</summary>
public sealed record MenuItemInput(
    Guid CategoryId,
    string Name,
    string? Description,
    string? Ingredients,
    decimal Price,
    int? ServesPeople,
    string? Portion,
    bool IsSpicy,
    bool IsVegan,
    bool IsLactoseFree,
    bool IsGlutenFree)
{
    public MenuItemDetails ToDetails() => new(Name, Description, Ingredients, Price, ServesPeople, Portion, IsSpicy);

    public DietaryAttributes ToDietary() => new(IsVegan, IsLactoseFree, IsGlutenFree);
}

public sealed class MenuItemInputValidator : AbstractValidator<MenuItemInput>
{
    public MenuItemInputValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage(UserMessages.Required);
        RuleFor(x => x.Name).NotEmpty().WithMessage(UserMessages.Required)
            .MaximumLength(MenuItemDetails.MaxNameLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.Description).MaximumLength(MenuItemDetails.MaxTextLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.Ingredients).MaximumLength(MenuItemDetails.MaxTextLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.Portion).MaximumLength(MenuItemDetails.MaxPortionLength).WithMessage(UserMessages.TooLong);
        RuleFor(x => x.Price).InclusiveBetween(0, MenuItemDetails.MaxPrice).WithMessage(UserMessages.InvalidValue);
        RuleFor(x => x.ServesPeople).InclusiveBetween(1, 50).When(x => x.ServesPeople.HasValue)
            .WithMessage(UserMessages.InvalidValue);
    }
}
