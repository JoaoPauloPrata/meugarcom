# @seugarcom/api

Backend do Meu Garçom em **.NET 10 / C#**, com Clean Architecture
(`.claude/skills/clean-architecture/SKILL.md`). Arquitetura, endpoints e decisões em
[`docs/backend.md`](../../docs/backend.md); regras do chat em [`docs/guardrails.md`](../../docs/guardrails.md).

```
apps/api/
  MeuGarcom.sln
  src/
    MeuGarcom.Domain/          # entidades com comportamento, Result, value objects
    MeuGarcom.Application/     # casos de uso, guardrails, prompts, catálogos de mensagens
    MeuGarcom.Infrastructure/  # EF Core (Supabase Postgres), provedores de IA, DI
    MeuGarcom.Api/             # Minimal API, endpoints finos, Program.cs
  tests/
    MeuGarcom.Domain.Tests/
    MeuGarcom.Application.Tests/
    MeuGarcom.IntegrationTests/   # OpenAI real; pulados sem OPENAI_API_KEY
```

## Configuração

Copie `.env.example` para `.env` (gitignored). Em desenvolvimento a API carrega esse arquivo;
em produção use os secrets do host.

| Variável | Uso |
|---|---|
| `OPENAI_API_KEY` | provedor principal |
| `ANTHROPIC_API_KEY` | opcional; habilita o fallback |
| `SUPABASE_URL` | issuer do JWT do Supabase Auth |
| `ConnectionStrings__Database` | Postgres com o papel `meugarcom_api` (pooler, session mode) |
| `CORS_ORIGINS` | origens do front, separadas por vírgula |

## Comandos

```sh
npm run dev:api     # na raiz: sobe em http://localhost:5080
npm run test:api    # todos os testes
```

## Deploy (Fly.io)

App `meugarcom-api` (região `gru`), configurado em `fly.toml` + `Dockerfile`. Todo push na
`main` que altera `apps/api/**` dispara `.github/workflows/deploy-api.yml`: roda os testes
(sem os de integração) e faz `flyctl deploy --remote-only` com o secret `FLY_API_TOKEN`.
Deploy manual: `flyctl deploy` a partir de `apps/api/`. Segredos: `flyctl secrets set -a meugarcom-api`.

Todo o código é em inglês. Só textos exibidos ao usuário final são em PT-BR, isolados em
`UserMessages`, `FixedReplies` e nos prompts. Ver `CLAUDE.md`.
