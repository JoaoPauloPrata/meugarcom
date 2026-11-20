using FluentValidation;
using MeuGarcom.Application.Common.Messages;

namespace MeuGarcom.Application.Menus.Commands.SetMenuItemImage;

public sealed class SetMenuItemImageValidator : AbstractValidator<SetMenuItemImageCommand>
{
    public const int MaxFileBytes = 5 * 1024 * 1024;

    public SetMenuItemImageValidator()
    {
        var invalidImage = UserMessages.ForCode("menu.item_image_invalid");
        RuleFor(x => x.Content).NotEmpty().WithMessage(invalidImage)
            .Must(c => c.Length <= MaxFileBytes).WithMessage(invalidImage);
        RuleFor(x => x).Must(x => MatchesSignature(x.ContentType, x.Content)).WithMessage(invalidImage)
            .When(x => x.Content is { Length: > 0 });
    }

    /// <summary>Checks the file bytes, not only the declared type, so only real images reach the public bucket.</summary>
    private static bool MatchesSignature(string contentType, byte[] content) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => content.AsSpan().StartsWith((byte[])[0xFF, 0xD8, 0xFF]),
        "image/png" => content.AsSpan().StartsWith((byte[])[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        "image/webp" => content.Length >= 12
            && content.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && content.AsSpan(8, 4).SequenceEqual("WEBP"u8),
        _ => false,
    };
}
