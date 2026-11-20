using FluentValidation;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Domain.Menus;

namespace MeuGarcom.Application.Menus.Commands.CreateCategory;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator() =>
        RuleFor(x => x.Name).NotEmpty().WithMessage(UserMessages.Required)
            .MaximumLength(MenuCategory.MaxNameLength).WithMessage(UserMessages.TooLong);
}
