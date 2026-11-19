# Backend (.NET)

O backend em `apps/api/` é escrito em .NET / C# e segue **Clean Architecture** conforme a skill
`.claude/skills/clean-architecture/SKILL.md`. A skill é a referência; este documento só
aterrissa os nomes no contexto do Meu Garçom.

## Projetos

| Projeto | Responsabilidade | Referencia |
|---|---|---|
| `MeuGarcom.Domain` | Entidades com comportamento (MVP: `Account`, `Restaurant`, `Unit`, `Menu`, `MenuCategory`, `MenuItem`, `MenuImport`, `Table`, `Conversation`, `AiModelSettings`; ver glossário em `produto.md`), enums, `Result`, exceções de domínio | nada |
| `MeuGarcom.Application` | Casos de uso (um command/query por classe), `IAppDbContext`, validators, pipeline behaviors | Domain |
| `MeuGarcom.Infrastructure` | `AppDbContext` (EF Core + Postgres/Supabase), configurações de entidade, migrations, serviços externos, `AddInfrastructure` | Application, Domain |
| `MeuGarcom.Api` | Minimal API, `IEndpointGroup` por feature, `Program.cs` | todos (via abstrações) |

## Idioma

Todo o código é em inglês: identificadores, namespaces, tabelas, colunas, migrations,
comentários, logs e mensagens de exceção. Só o texto lido pelo usuário final (respostas do
assistente, mensagens de erro exibidas) é em PT-BR, isolado em catálogos de mensagens ou
prompts. Ver seção "Idioma" no `CLAUDE.md`.

## Regras que não se negocia

1. Dependências apontam para dentro. O compilador garante via project references.
2. Regra de negócio vive na entidade. Handler orquestra; endpoint traduz HTTP.
3. `IAppDbContext` com `DbSet<T>`. Repositório só para query complexa e reutilizável.
4. Infraestrutura é plugin: trocar EF/Postgres por outra coisa não toca Domain/Application.
5. Endpoint fino: recebe request, manda para `ISender`, converte `Result` em resposta.

## Organização por feature

```
MeuGarcom.Application/
  Menus/
    Commands/CreateMenuItem/{CreateMenuItemCommand,CreateMenuItemHandler,CreateMenuItemValidator}.cs
    Queries/GetMenu/{GetMenuQuery,GetMenuHandler,MenuDto}.cs
  Conversations/
    Commands/SendMessage/{SendMessageCommand,SendMessageHandler,SendMessageValidator}.cs
MeuGarcom.Api/
  Endpoints/MenuEndpoints.cs
  Endpoints/ConversationEndpoints.cs
```

`Order` e `Payment` só entram nas modalidades 2 e 3 (fora do MVP).

## IA como plugin

```
MeuGarcom.Application/Common/Interfaces/IWaiterAssistant.cs      # contrato único
MeuGarcom.Infrastructure/Ai/
  OpenAiWaiterAssistant.cs          # provedor principal
  AnthropicWaiterAssistant.cs       # fallback
  FallbackWaiterAssistant.cs        # decorator: lê AiModelSettings do banco, escolhe o
                                    # provedor/modelo default, cai para o outro em erro,
                                    # timeout ou rate limit
```

Provedor e modelo default ficam em `AiModelSettings` no banco (nunca hardcoded). Chaves de
API ficam em variáveis de ambiente/secrets, nunca no banco nem no repo. Ao implementar a
parte Anthropic, carregue a skill `claude-api`.

Os guardrails do chat ([guardrails.md](guardrails.md)) seguem a mesma divisão. As regras de
negócio ficam no domínio (ex.: `ComboSuggestion` rejeita total fora da faixa de tolerância
da unidade; `IsVegan`, `IsLactoseFree` e `IsGlutenFree` de `MenuItem` só mudam por ação
manual). A validação de entrada e de saída do
modelo fica em Application, no caso de uso do chat. O classificador de entrada é mais um
plugin de IA em Infrastructure.

## Banco

Postgres hospedado no Supabase. O schema compartilhado com o front fica em `supabase/`.
Migrations do EF Core ficam em `MeuGarcom.Infrastructure/Persistence/Migrations/`.
