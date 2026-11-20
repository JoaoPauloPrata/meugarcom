using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Menus;

public static class MenuErrors
{
    public static readonly Error MenuNotFound = Error.NotFound("menu.not_found");
    public static readonly Error CategoryNotFound = Error.NotFound("menu.category_not_found");
    public static readonly Error CategoryNameInvalid = Error.Validation("menu.category_name_invalid");
    public static readonly Error CategoryNameDuplicated = Error.Conflict("menu.category_name_duplicated");
    public static readonly Error ItemNotFound = Error.NotFound("menu.item_not_found");
    public static readonly Error ItemNameInvalid = Error.Validation("menu.item_name_invalid");
    public static readonly Error ItemTextTooLong = Error.Validation("menu.item_text_too_long");
    public static readonly Error ItemPortionInvalid = Error.Validation("menu.item_portion_invalid");
    public static readonly Error ItemPriceInvalid = Error.Validation("menu.item_price_invalid");
    public static readonly Error ItemServesPeopleInvalid = Error.Validation("menu.item_serves_people_invalid");
    public static readonly Error ItemImageInvalid = Error.Validation("menu.item_image_invalid");
    public static readonly Error TooManyItems = Error.Validation("menu.too_many_items");
    public static readonly Error ImportNotFound = Error.NotFound("menu.import_not_found");
    public static readonly Error ImportNotDraft = Error.Conflict("menu.import_not_draft");
    public static readonly Error ImportEmpty = Error.Validation("menu.import_empty");
}
