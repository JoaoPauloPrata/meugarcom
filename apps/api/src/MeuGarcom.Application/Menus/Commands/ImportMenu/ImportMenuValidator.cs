using FluentValidation;
using MeuGarcom.Application.Common.Messages;

namespace MeuGarcom.Application.Menus.Commands.ImportMenu;

public sealed class ImportMenuValidator : AbstractValidator<ImportMenuCommand>
{
    public const int MaxFileBytes = 10 * 1024 * 1024;

    public static readonly IReadOnlySet<string> AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp",
        "text/csv",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    };

    public ImportMenuValidator()
    {
        var invalidFile = UserMessages.ForCode("menu.import_file_invalid");
        RuleFor(x => x.ContentType).Must(AllowedContentTypes.Contains).WithMessage(invalidFile);
        RuleFor(x => x.Content).NotEmpty().WithMessage(invalidFile)
            .Must(c => c.Length <= MaxFileBytes).WithMessage(invalidFile);
    }
}
