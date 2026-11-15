# @seugarcom/api

Backend do Meu Garçom em **.NET / C#**.

**Deve seguir os princípios da skill `clean-architecture`**
(`.claude/skills/clean-architecture/SKILL.md`). Detalhes em `docs/backend.md`.

Todo o código é em inglês (identificadores, tabelas, comentários, logs). Só textos
exibidos ao usuário final são em PT-BR. Ver `CLAUDE.md`.

Layout previsto:

```
apps/api/
  MeuGarcom.sln
  src/
    MeuGarcom.Domain/          # entidades com comportamento, enums, Result, exceções
    MeuGarcom.Application/     # casos de uso (commands/queries), IAppDbContext, validators
    MeuGarcom.Infrastructure/  # EF Core (Postgres/Supabase), serviços externos, DI
    MeuGarcom.Api/             # Minimal API, endpoints finos, Program.cs
  tests/
```

Hoje a persistência da waitlist é feita direto no Supabase pelo front
(`apps/web/src/lib/supabase.js`). O schema fica em `supabase/` na raiz do repo.
