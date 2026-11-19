---
name: git-flow
description: >
  Fluxo de git, branches, deploy e migrations do Meu Garçom. main = produção; todo
  desenvolvimento acontece em branch a partir da main e é testado localmente. Carregue esta
  skill SEMPRE que: receber uma nova demanda de código, for criar/trocar branch, commitar,
  ou quando o usuário pedir "envie para produção", "aplique", "deploy", "sobe", "publica",
  "manda pra main" ou similares.
---

# Git flow do Meu Garçom

## Regras fixas

1. **`main` é produção.** Nunca commitar direto na `main`. Nunca fazer push para `main` sem
   o usuário pedir explicitamente para enviar para produção.
2. **Todo trabalho acontece em uma branch criada a partir da `main` atualizada**, e é testado
   em ambiente local (não há staging por enquanto).
3. **Nova demanda com branch em andamento → SEMPRE perguntar antes de qualquer coisa:**

   > Devo criar uma nova branch ou utilizar a mesma?

   Nunca assumir a resposta. O usuário às vezes separa por feature e às vezes agrupa várias
   mudanças num "pacotão" numa única branch. Use `AskUserQuestion` com as duas opções
   (usar a branch atual `<nome>` / criar nova a partir da `main`) e só então comece.
4. **Deploy só quando o usuário mandar** ("envie para produção", "aplique", "deploy",
   "manda pra main" etc.). Até lá, a branch fica local/remota sem merge.

## Começando uma demanda

```sh
git status                      # há branch em andamento? (qualquer branch != main com commits
                                #   ou alterações pendentes conta como "em andamento")
git checkout main && git pull   # só se for criar branch nova
git checkout -b <tipo>/<descricao-curta>   # ex.: feat/cardapio-api, fix/waitlist-email
```

- Se já existe branch em andamento: **pergunte** (regra 3). Se a resposta for "mesma",
  continue nela; se for "nova", crie a partir da `main` atualizada.
- Commits curtos, no imperativo, em português (ex.: "Adiciona entidade Order"). Commitar
  conforme o trabalho avança; não esperar o fim da demanda.

## Enviando para produção

Quando o usuário disser "envie para produção" / "aplique" / "deploy" / "manda pra main":

1. **Garantir que a branch está íntegra**: build do que mudou (`npm run build` para o front,
   `dotnet build`/`dotnet test` para a API). Se falhar, parar e reportar; não fazer merge.
2. **Merge na `main`**:
   ```sh
   git checkout main && git pull
   git merge --no-ff <branch> -m "Merge <branch>: <resumo>"
   git push origin main
   ```
   Em caso de conflito, resolver e mostrar ao usuário o que foi decidido.
3. **Front (Vercel)**: redeploya sozinho no push para `main`. Nada a fazer além do push;
   apenas avisar que o deploy foi disparado.
4. **API (Fly.io)**: verificar se existe workflow em `.github/workflows/` que faz o deploy.
   - Se existir: o push já dispara; acompanhar o resultado (`gh run list` / `gh run watch`).
   - Se NÃO existir: fazer o deploy manual com `flyctl deploy` a partir de `apps/api/`
     (ou onde estiver o `fly.toml`).
5. **Migrations (Supabase)**: executar as migrations pendentes via MCP do Supabase
   (`apply_migration`). **O usuário autorizou isso de forma permanente**; não é preciso
   pedir de novo. Antes de aplicar: listar migrations já aplicadas (`list_migrations`),
   aplicar só o que falta, em ordem, e conferir com `list_tables`/`get_advisors` depois.
   Se a migration for destrutiva (drop de tabela/coluna com dados), avisar antes mesmo assim.
6. **Reportar**: branch mergeada, commit na `main`, status do deploy do front e da API,
   migrations aplicadas (nome de cada uma). Se algum passo falhou, dizer qual e por quê.
7. **Limpar**: depois do merge, perguntar se pode apagar a branch (local e remota). Não
   apagar sem perguntar.

## Ordem de deploy quando há migration

Migrations que **adicionam** (tabela/coluna nova, nullable) podem rodar antes do deploy da
API. Migrations que **removem ou renomeiam** só depois que a API nova estiver no ar. Em
dúvida: migration aditiva → antes; destrutiva → depois; e avisar o usuário.

## O que NÃO fazer

- Não commitar na `main`.
- Não fazer merge/push para `main` sem pedido explícito.
- Não criar nova branch nem reaproveitar a atual sem perguntar quando já existe uma em
  andamento.
- Não usar `--force` em `main`.
- Não apagar branches sem perguntar.
- Não pular build/teste antes do merge.
