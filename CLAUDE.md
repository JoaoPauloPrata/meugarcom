# Meu Garçom — guia para o Claude Code

Monorepo (npm workspaces).

## Produto

**Meu Garçom** (nome definitivo; `seugarcom` é só o slug do repo) é um **AI Waiter**: um garçom de IA que conhece o cardápio do restaurante e conversa com o
cliente na mesa (QR code / NFC) de forma fluida e humanizada, **respondendo apenas sobre o
cardápio**. Responde perguntas ("tem prato sem glúten?", "qual a maior pizza?"), monta
combos por orçamento ("combo de sushi para dois de R$ 150") e faz sugestões de
acompanhamento ("gostaria de uma Coca?").

Três modalidades, e a 1 é o core incluído em todas:

1. **Assistente** — só o chat; o pedido continua com o garçom humano. **← MVP, único escopo atual.**
2. **+ Pedidos** — cliente pede pelo chat; pedidos caem num sistema da cozinha. Sem pagamento.
3. **+ Pagamento** — Pix gerado no chat; pagamentos notificados na sessão do restaurante.

Decisões fixas: cobrança **por unidade** (Conta → Restaurante → Unidade); cardápio entra
por **importação com ajustes manuais**; IA com **OpenAI** principal, **Anthropic** fallback
e modelo default **configurável via banco** (nunca hardcoded).

Detalhes, glossário de domínio e perguntas em aberto em `docs/produto.md`. **Não implementar
nada das modalidades 2 e 3 (pedido, cozinha, pagamento) sem o usuário pedir.**

**Guardrails do chat** (anti-alucinação, prompt injection, combos, escopo, idioma) estão em
`docs/guardrails.md`, leitura obrigatória antes de mexer no assistente. O AI Waiter só
informa o que está explícito no cardápio, nunca inferido pelo nome ou pela receita.

```
apps/web/     front (Vite + React) — landing page e waitlist
apps/api/     backend em .NET (C#)
supabase/     schema SQL compartilhado (Postgres no Supabase)
docs/         documentação do projeto
.claude/skills/  skills do projeto (carregue antes de mexer na área correspondente)
```

## Backend: .NET com Clean Architecture (obrigatório)

O backend em `apps/api/` é escrito em **.NET / C#** e **deve seguir os princípios da skill
`clean-architecture`** (`.claude/skills/clean-architecture/SKILL.md`). Leia a skill antes de
criar ou alterar qualquer código em `apps/api/`.

Resumo do que a skill exige (a skill é a fonte da verdade, não este resumo):

- **4 projetos**: `MeuGarcom.Domain`, `MeuGarcom.Application`, `MeuGarcom.Infrastructure`,
  `MeuGarcom.Api`, em `apps/api/src/`. Testes em `apps/api/tests/`.
- **Dependências apontam para dentro**: Domain não referencia nada; Application referencia só
  Domain; Infrastructure referencia Application e Domain; Api referencia todos mas depende de
  abstrações.
- **Domínio rico**: regras de negócio vivem nas entidades (métodos, fábricas estáticas,
  `Result`), nunca em handlers ou endpoints. Sem modelo anêmico.
- **Um caso de uso por classe** (command/query + handler + validator), em pastas por feature.
  Nada de `XService` com dezenas de métodos.
- **`IAppDbContext` em vez de repositório por entidade**. Repositório só para query complexa
  e reutilizável.
- **Infraestrutura é plugin**: EF Core, Supabase/Postgres, e-mail, storage ficam em
  Infrastructure implementando interfaces de Application/Domain.
- **Endpoints finos** (Minimal API, `IEndpointGroup` auto-descoberto): mapeiam HTTP para caso
  de uso e o `Result` para resposta HTTP. Zero lógica de negócio.
- **IA é plugin**: `IWaiterAssistant` em Application; `OpenAiWaiterAssistant` e
  `AnthropicWaiterAssistant` em Infrastructure; um orquestrador escolhe o provedor pela
  configuração no banco e faz o fallback. Ao implementar a parte Anthropic, carregue a
  skill `claude-api`.

Ao propor estrutura, nomes ou código do backend, cite qual princípio da skill está sendo
aplicado. Se uma decisão fugir da skill, explique o motivo antes de implementar.

## Front: skill `ui-ux-pro-max` (obrigatória)

`apps/web/` usa Vite + React 19 e fala com o Supabase direto pelo client em
`apps/web/src/lib/supabase.js`. Variáveis de ambiente em `apps/web/.env`
(modelo em `apps/web/.env.example`).

**Toda alteração de frontend deve ser feita E validada usando a skill `ui-ux-pro-max`**
(plugin global, invocada via `Skill` com `ui-ux-pro-max:ui-ux-pro-max`). Isso vale para
páginas, componentes, layout, responsividade, tipografia, cores, acessibilidade e interação.

- **Antes de mexer**: carregue a skill e consulte as guidelines/estilos/paletas dela para a
  decisão em questão. Não comece a editar JSX/CSS sem ter carregado a skill.
- **Depois de mexer**: revise o resultado contra as guidelines de UX da skill (hierarquia,
  contraste, responsividade, estados, acessibilidade) e registre no resumo final o que foi
  verificado. Sempre que possível, abra a página localmente (`npm run dev`) e confira.
- Se uma decisão visual fugir do que a skill recomenda, explique o motivo antes de
  implementar.

## Comandos

```sh
npm install        # instala todos os workspaces
npm run dev        # sobe o front
npm run build      # build do front
```

## Git flow (obrigatório)

Siga a skill `git-flow` (`.claude/skills/git-flow/SKILL.md`). Resumo:

- `main` é produção. Nunca commitar nem fazer push direto nela.
- Toda demanda é feita em branch a partir da `main` e testada localmente.
- **Se já existe branch em andamento e chega nova demanda, SEMPRE pergunte antes:
  "Devo criar uma nova branch ou utilizar a mesma?"** Nunca assuma.
- Só quando o usuário mandar ("envie para produção", "aplique", "deploy"): merge na `main`,
  push (Vercel redeploya o front sozinho), deploy da API via `flyctl` se não houver GitHub
  Actions, e migrations via MCP do Supabase (autorização permanente do usuário).

## Idioma (obrigatório)

**Todo código é em inglês, por padrão e sem exceção.** Isso vale para o .NET inteiro e para
o código do front: identificadores, classes, métodos, variáveis, arquivos, pastas, projetos,
namespaces, nomes de tabelas e colunas, migrations, enums, comentários, logs, mensagens de
exceção, nomes de endpoints e rotas, chaves de JSON, testes.

**Só o texto que a pessoa lê na tela é em PT-BR**: strings de UI do front, respostas do AI
Waiter ao cliente, mensagens de validação e de erro que chegam ao usuário final, e-mails.
Esses textos ficam isolados (constantes/arquivos de mensagens ou prompts), nunca misturados
com identificadores.

```csharp
// GOOD
public sealed class MenuItem { public Money Price { get; private set; } }
return Result.Failure(MenuErrors.ItemNotFound);        // código em inglês
// mensagem exibida ao cliente, em PT-BR, vinda de um catálogo de mensagens:
// MenuErrors.ItemNotFound => "Não encontrei esse item no cardápio."

// BAD
public sealed class ItemCardapio { public decimal Preco { get; set; } }
// busca o item no cardápio  ← comentário em português
```

Exceção legada: a tabela `public.waitlist` já em produção tem colunas em português
(`nome`, `restaurante`, `cidade`...). Não renomear sem o usuário pedir; tabelas novas
seguem o padrão em inglês.

Documentação em `docs/`, `CLAUDE.md` e mensagens de commit continuam em português.

## Convenções

- Commits curtos no imperativo (ex.: "Adiciona entidade Order").
- Não commitar `.env`. Segredos do Supabase ficam fora do repo.
