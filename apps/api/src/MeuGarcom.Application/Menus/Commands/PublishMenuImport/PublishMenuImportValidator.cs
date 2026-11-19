using FluentValidation;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.PublishMenuImport;

public sealed class PublishMenuImportValidator : AbstractValidator<PublishMenuImportCommand>
{
    public PublishMenuImportValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage(UserMessages.ForCode("menu.import_empty"))
            .Must(items => items.Count <= Menu.MaxItems).WithMessage(UserMessages.ForCode("menu.too_many_items"));
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.CategoryName).NotEmpty().WithMessage(UserMessages.Required)
                .MaximumLength(MenuCategory.MaxNameLength).WithMessage(UserMessages.TooLong);
            item.RuleFor(i => i.Name).NotEmpty().WithMessage(UserMessages.Required)
                .MaximumLength(MenuItemDetails.MaxNameLength).WithMessage(UserMessages.TooLong);
            item.RuleFor(i => i.Price).InclusiveBetween(0, MenuItemDetails.MaxPrice).WithMessage(UserMessages.InvalidValue);
        });
    }
}
