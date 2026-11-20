using FluentValidation;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.RenameCategory;

public sealed class RenameCategoryValidator : AbstractValidator<RenameCategoryCommand>
{
    public RenameCategoryValidator() =>
        RuleFor(x => x.Name).NotEmpty().WithMessage(UserMessages.Required)
            .MaximumLength(MenuCategory.MaxNameLength).WithMessage(UserMessages.TooLong);
}
