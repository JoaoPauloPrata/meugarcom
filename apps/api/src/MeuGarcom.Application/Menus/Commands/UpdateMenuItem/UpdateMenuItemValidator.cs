using FluentValidation;

namespace MeuGarcom.Application.Menus.Commands.UpdateMenuItem;

public sealed class UpdateMenuItemValidator : AbstractValidator<UpdateMenuItemCommand>
{
    public UpdateMenuItemValidator() =>
        RuleFor(x => x.Item).NotNull().SetValidator(new MenuItemInputValidator());
}
