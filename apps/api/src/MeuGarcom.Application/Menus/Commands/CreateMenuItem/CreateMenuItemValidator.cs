using FluentValidation;

namespace MeuGarcom.Application.Menus.Commands.CreateMenuItem;

public sealed class CreateMenuItemValidator : AbstractValidator<CreateMenuItemCommand>
{
    public CreateMenuItemValidator() =>
        RuleFor(x => x.Item).NotNull().SetValidator(new MenuItemInputValidator());
}
