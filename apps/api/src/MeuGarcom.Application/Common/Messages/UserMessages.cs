namespace MeuGarcom.Application.Common.Messages;

/// <summary>
/// Catalog of texts shown to the end user (PT-BR). Keyed by stable error codes so that the
/// domain and the handlers never carry user-facing strings.
/// </summary>
public static class UserMessages
{
    private static readonly Dictionary<string, string> ErrorTexts = new()
    {
        ["account.owner_required"] = "Sessão inválida. Entre novamente.",
        ["account.name_invalid"] = "Informe seu nome (até 120 caracteres).",
        ["account.already_exists"] = "Esta conta já tem um restaurante cadastrado.",
        ["account.not_found"] = "Conclua o cadastro do seu restaurante para continuar.",
        ["restaurant.name_invalid"] = "Informe o nome do restaurante (até 160 caracteres).",
        ["restaurant.cuisine_type_invalid"] = "O tipo de cozinha deve ter até 60 caracteres.",
        ["unit.not_found"] = "Unidade não encontrada.",
        ["unit.name_invalid"] = "Informe o nome da unidade (até 120 caracteres).",
        ["unit.city_invalid"] = "A cidade deve ter até 120 caracteres.",
        ["unit.combo_tolerance_out_of_range"] = "A tolerância do combo deve ficar entre 0% e 50%.",
        ["unit.chat_suggestion_text_invalid"] = "Cada sugestão do chat deve ter de 1 a 80 caracteres.",
        ["unit.chat_suggestion_icon_invalid"] = "Escolha um ícone válido para a sugestão.",
        ["unit.too_many_chat_suggestions"] = "O chat aceita até 3 sugestões.",
        ["unit.table_count_invalid"] = "Informe quantas mesas deseja adicionar.",
        ["unit.too_many_tables"] = "O limite é de 300 mesas por unidade.",
        ["unit.table_not_found"] = "Mesa não encontrada.",
        ["menu.not_found"] = "Cardápio não encontrado.",
        ["menu.category_not_found"] = "Categoria não encontrada.",
        ["menu.category_name_invalid"] = "Informe o nome da categoria (até 80 caracteres).",
        ["menu.category_name_duplicated"] = "Já existe uma categoria com esse nome.",
        ["menu.item_not_found"] = "Não encontrei esse item no cardápio.",
        ["menu.item_name_invalid"] = "Informe o nome do prato (até 120 caracteres).",
        ["menu.item_text_too_long"] = "Descrição e ingredientes devem ter até 1000 caracteres.",
        ["menu.item_portion_invalid"] = "A porção deve ter até 60 caracteres.",
        ["menu.item_price_invalid"] = "Informe um preço válido, com até 2 casas decimais.",
        ["menu.item_serves_people_invalid"] = "\"Serve quantas pessoas\" deve ficar entre 1 e 50.",
        ["menu.item_image_invalid"] = "Envie uma foto JPG, PNG ou WEBP de até 5 MB.",
        ["menu.too_many_items"] = "O cardápio atingiu o limite de 600 itens.",
        ["menu.import_not_found"] = "Importação não encontrada.",
        ["menu.import_not_draft"] = "Esta importação já foi publicada ou descartada.",
        ["menu.import_empty"] = "Nenhum item para publicar.",
        ["menu.import_file_invalid"] = "Envie um PDF, uma foto (JPG, PNG, WEBP) ou uma planilha (XLSX, CSV) de até 10 MB.",
        ["menu.import_failed"] = "Não consegui ler esse arquivo. Tente uma foto mais nítida ou outro formato.",
        ["conversation.not_found"] = "Conversa não encontrada. Abra o chat novamente pelo QR code.",
        ["conversation.access_code_invalid"] = "Este QR code não é válido. Peça ajuda ao garçom.",
        ["conversation.closed"] = "Esta conversa foi encerrada. Se precisar, chame o garçom.",
        ["conversation.expired"] = "Esta conversa expirou. Abra o chat novamente pelo QR code.",
        ["conversation.message_limit_reached"] = "Esta conversa atingiu o limite de mensagens. Abra o chat novamente pelo QR code.",
        ["conversation.message_empty"] = "Escreva sua pergunta.",
        ["conversation.message_too_long"] = "Sua mensagem está muito longa. Tente resumir em até 500 caracteres.",
    };

    public const string Unexpected = "Algo deu errado. Tente novamente em instantes.";
    public const string Unauthorized = "Sua sessão expirou. Entre novamente.";
    public const string ValidationTitle = "Revise os dados informados.";

    public const string Required = "Campo obrigatório.";
    public const string TooLong = "Texto muito longo.";
    public const string InvalidValue = "Valor inválido.";

    public static string ForCode(string code) => ErrorTexts.GetValueOrDefault(code, Unexpected);
}
