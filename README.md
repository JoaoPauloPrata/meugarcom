# Meu Garçom

AI Waiter para restaurantes. Monorepo (npm workspaces). Ver `docs/produto.md`.

```
apps/
  web/        front (Vite + React) — landing page e waitlist (alterações via skill ui-ux-pro-max)
  api/        backend em .NET (Clean Architecture — ver docs/backend.md)
supabase/     schema SQL do Supabase, compartilhado
docs/         documentação do projeto
.claude/skills/  skills do projeto (ex.: clean-architecture, obrigatória no backend)
```

## Rodando

```sh
npm install          # instala todos os workspaces
npm run dev          # sobe o front (apps/web)
npm run build        # build do front
```

Variáveis de ambiente do front ficam em `apps/web/.env`
(veja `apps/web/.env.example`).
