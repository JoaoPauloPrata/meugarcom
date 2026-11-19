# Backend (.NET)

O backend em `apps/api/` é escrito em .NET 10 / C# e segue **Clean Architecture** conforme a
skill `.claude/skills/clean-architecture/SKILL.md`. A skill é a referência; este documento só
aterrissa os nomes no contexto do Meu Garçom.

## Projetos

| Projeto | Responsabilidade | Referencia |
|---|---|---|
| `MeuGarcom.Domain` | Entidades com comportamento (`Account`, `Restaurant`, `Unit`, `DiningTable`, `Menu`, `MenuCategory`, `MenuItem`, `MenuImport`, `Conversation`, `ConversationMessage`, `GuardrailEvent`, `AiModelSettings`), value objects (`ComboSuggestion`, `MenuItemDetails`, `DietaryAttributes`), `Result`/`Error` | nada |
| `MeuGarcom.Application` | Casos de uso (um command/query por pasta, com handler e validator), `IAppDbContext`, `IWaiterAssistant`, `ICurrentUser`, pipeline de validação, guardrails do chat, prompts, catálogos de mensagens | Domain |
| `MeuGarcom.Infrastructure` | `AppDbContext` (EF Core + Npgsql, snake_case), configurações de entidade, provedores de IA (OpenAI, Anthropic) e o orquestrador com fallback, `AddInfrastructure` | Application, Domain |
| `MeuGarcom.Api` | Minimal API, `IEndpointGroup` por feature (auto-descoberto), JWT do Supabase, `Result` → ProblemDetails, `Program.cs` | todos |

Testes em `apps/api/tests/`: `Domain.Tests` (regras), `Application.Tests` (guardrails de entrada e
saída), `IntegrationTests` (chamadas reais à OpenAI com cardápio em memória; pulados sem chave).

Mediator: [`Mediator`](https://github.com/martinothamar/Mediator) (source generator, MIT). O
gerador fica no projeto Api e varre o assembly de Application.

## Regras que não se negocia

1. Dependências apontam para dentro. O compilador garante via project references.
2. Regra de negócio vive na entidade. Handler orquestra; endpoint traduz HTTP.
3. `IAppDbContext` com `DbSet<T>`. Repositório só para query complexa e reutilizável.
4. Infraestrutura é plugin: trocar EF/Postgres por outra coisa não toca Domain/Application.
5. Endpoint fino: recebe request, manda para `ISender`, converte `Result` em resposta.

## Organização por feature

```
MeuGarcom.Application/
  Accounts/      CompleteOnboarding, GetWorkspace
  Units/         UpdateUnitSettings, GetUnitOverview
  Tables/        AddTables, RemoveTable, ListTables
  Menus/         GetMenu, Create/Rename/DeleteCategory, Create/Update/DeleteMenuItem,
                 SetMenuItemAvailability, ImportMenu, PublishMenuImport
  Conversations/ StartConversation, SendMessage, GetPublicMenu, ListConversations
    Waiter/      InputGuard, WaiterTurn, OutputGuard, MenuSnapshot, WaiterPrompt, FixedReplies
  Common/        Interfaces, Behaviors, Guardrails (InjectionPatterns), Messages (UserMessages),
                 Persistence (consultas de posse e de cardápio)
MeuGarcom.Api/Endpoints/
  AccountEndpoints, UnitEndpoints, MenuEndpoints, PublicChatEndpoints
```

`Order` e `Payment` só entram nas modalidades 2 e 3 (fora do MVP).

## Endpoints

| Rota | Auth | Caso de uso |
|---|---|---|
| `GET /api/me/workspace` | JWT | conta, restaurante e unidades do dono |
| `POST /api/onboarding` | JWT | cria conta → restaurante → unidade → cardápio → mesas |
| `GET/PUT /api/units/{id}/overview`, `/settings` | JWT | visão geral e configurações |
| `GET/POST/DELETE /api/units/{id}/tables[/{tableId}]` | JWT | mesas e QR codes |
| `GET /api/units/{id}/conversations` | JWT | conversas recentes (leitura) |
| `/api/units/{id}/menu/...` | JWT | cardápio, categorias, itens, importação e publicação |
| `POST /api/public/conversations` | — | abre conversa nova pelo código do QR (mesa ou unidade) |
| `POST /api/public/conversations/{id}/messages` | — | mensagem do cliente → resposta do AI Waiter |
| `GET /api/public/menus/{code}` | — | cardápio completo exibido no chat |

## Autenticação

O front usa o **Supabase Auth** (e-mail/senha; Google via OAuth quando o provedor for habilitado
no painel do Supabase). A API valida o JWT do Supabase (ES256) pelo OIDC discovery do issuer
`{SUPABASE_URL}/auth/v1`, audiência `authenticated`. O `sub` do token é o `Account.OwnerUserId`;
toda query do painel filtra por posse (`UnitsOwnedBy`).

## IA como plugin

```
MeuGarcom.Application/Common/Interfaces/IWaiterAssistant.cs   # contrato único, por AiTask
MeuGarcom.Infrastructure/Ai/
  OpenAiWaiterAssistant.cs      # provedor principal (Chat Completions + Structured Outputs)
  AnthropicWaiterAssistant.cs   # fallback (SDK oficial, structured outputs)
  FallbackWaiterAssistant.cs    # orquestrador: lê AiModelSettings, escolhe modelo por tarefa,
                                # cai para o fallback em erro, timeout, rate limit ou recusa
  AiSettingsProvider.cs         # override por restaurante > global, cache de 1 minuto
```

`IWaiterAssistant.CompleteAsync` recebe um `AiCompletionRequest` neutro (tarefa, instruções,
dados delimitados, mensagens, schema JSON, anexo opcional) e devolve o JSON validado pelo schema.
O mesmo contrato serve às três tarefas: resposta do garçom (`WaiterReply`), classificador de
entrada (`InputClassification`) e importação de cardápio (`MenuExtraction`). Os prompts ficam em
Application; a Infrastructure só transporta. Assim o fallback é implementado uma vez só.

Provedor e modelo ficam em `ai_model_settings` (nunca no código). Seed atual: principal
`openai/gpt-5.4-mini` (`reasoning_effort: none`), fallback `anthropic/claude-opus-5-5`
(`effort: low`), classificador `openai/gpt-5.4-nano`. Trocar = `update` na tabela. O fallback só
funciona com `ANTHROPIC_API_KEY` configurada; sem ela é ignorado com aviso no log.

Chaves de API ficam em variáveis de ambiente/secrets, nunca no banco nem no repo.

## Banco

Postgres no Supabase. **O schema é dono das migrations SQL em `supabase/migrations/`**,
aplicadas pelo MCP do Supabase (decisão de 2026-11-18). O EF Core só mapeia as tabelas
(`UseSnakeCaseNamingConvention`), sem migrations do EF.

- A API conecta com o papel dedicado `meugarcom_api` (menor privilégio: CRUD só nas tabelas do
  app, leitura em `ai_model_settings`, sem acesso a `waitlist` nem a `auth`), pelo pooler do
  Supabase em session mode. A senha é definida fora das migrations e fica só no `.env`/secrets.
- RLS está ligado em todas as tabelas do app sem policies: `anon` e `authenticated` não têm
  acesso pelo PostgREST. Todo acesso passa pela API.
- IDs são UUID v7 gerados pelo domínio.

## Rodando

```sh
cp apps/api/.env.example apps/api/.env   # preencha as chaves
npm run dev:api                           # http://localhost:5080
npm run test:api                          # testes (os de integração chamam a OpenAI)
```
