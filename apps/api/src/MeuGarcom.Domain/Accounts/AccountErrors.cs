using MeuGarcom.Domain.Common;

namespace MeuGarcom.Domain.Accounts;

public static class AccountErrors
{
    public static readonly Error OwnerRequired = Error.Validation("account.owner_required");
    public static readonly Error NameInvalid = Error.Validation("account.name_invalid");
    public static readonly Error AlreadyExists = Error.Conflict("account.already_exists");
    public static readonly Error NotFound = Error.NotFound("account.not_found");
}

public static class RestaurantErrors
{
    public static readonly Error NameInvalid = Error.Validation("restaurant.name_invalid");
    public static readonly Error CuisineTypeInvalid = Error.Validation("restaurant.cuisine_type_invalid");
}

public static class UnitErrors
{
    public static readonly Error NotFound = Error.NotFound("unit.not_found");
    public static readonly Error NameInvalid = Error.Validation("unit.name_invalid");
    public static readonly Error CityInvalid = Error.Validation("unit.city_invalid");
    public static readonly Error ComboToleranceOutOfRange = Error.Validation("unit.combo_tolerance_out_of_range");
    public static readonly Error ChatSuggestionTextInvalid = Error.Validation("unit.chat_suggestion_text_invalid");
    public static readonly Error ChatSuggestionIconInvalid = Error.Validation("unit.chat_suggestion_icon_invalid");
    public static readonly Error TooManyChatSuggestions = Error.Validation("unit.too_many_chat_suggestions");
    public static readonly Error TableCountInvalid = Error.Validation("unit.table_count_invalid");
    public static readonly Error TooManyTables = Error.Validation("unit.too_many_tables");
    public static readonly Error TableNotFound = Error.NotFound("unit.table_not_found");
}
