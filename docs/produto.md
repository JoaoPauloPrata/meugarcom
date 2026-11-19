# Produto: Meu Garçom (AI Waiter)

## Visão

Um **garçom de IA** que conhece o cardápio do estabelecimento e conversa com o cliente de
forma fluida e humanizada, como numa conversa com o Claude ou o ChatGPT, **mas responde
apenas sobre o cardápio**. O cliente acessa pela mesa (QR code ou tag NFC) e tem o AI Waiter
disponível durante toda a experiência no local.

Posicionamento: hospitalidade de verdade, não automação fria. O sistema cuida do caminho;
a equipe cuida das pessoas.

Público: restaurantes brasileiros. **Nome definitivo: Meu Garçom.** O slug `seugarcom`
permanece só como nome do repositório e dos pacotes npm.

## O que o AI Waiter faz

### Perguntas sobre o cardápio

Responde qualquer pergunta sobre itens, preços, porções, ingredientes, restrições e
preparo, sempre com base no cardápio cadastrado pelo restaurante. Exemplos:

- "Qual sua maior pizza?"
- "Qual seu hambúrguer mais caro?"
- "Quais opções de vinho?"
- "Tem prato sem glúten?"
- "A feijoada é apimentada?"
- "Qual serve duas pessoas?"

Fora do cardápio, o assistente recusa com educação e traz a conversa de volta. Ele só
informa o que está explícito no cardápio, sem inferir pelo nome do prato ou pela receita
tradicional. Regras completas em [guardrails.md](guardrails.md).

### Montagem de combos

O cliente descreve o que quer e um orçamento, e o assistente monta uma combinação
coerente dentro do valor. Exemplo: "Me sugira um combo de sushi para dois de 150 reais"
→ o assistente propõe itens do cardápio que somam até R$ 150 e servem duas pessoas. Se não
der, ele pode passar do orçamento em até 20% e avisa ("Não consegui com R$ 140, mas aqui
vai uma opção por R$ 155"). Ver [guardrails.md](guardrails.md#4-combos-por-orçamento).

### Sugestões (upsell contextual)

Quando o cliente menciona ou escolhe algo, o assistente oferece um acompanhamento
pertinente e já apresenta as opções. Exemplo: cliente pede hambúrguer e fritas → "Gostaria
de uma Coca para acompanhar?" com as opções de Coca do cardápio.

## Os três produtos

Donos de restaurante podem resistir a um sistema completo com pedidos e pagamento, porque
isso interfere na operação deles. Por isso o produto é escalonado em três modalidades. A
modalidade 1 é o **core** e está incluída em todas as outras.

| # | Modalidade | O que inclui | Pedido | Pagamento |
|---|---|---|---|---|
| 1 | **Assistente** | AI Waiter que sabe tudo do cardápio e responde de forma humanizada. O cliente continua pedindo com o garçom humano. | garçom humano | com o restaurante |
| 2 | **Assistente + Pedidos** | Tudo da 1, e o cliente faz o pedido direto no chat. Os pedidos chegam num **sistema da cozinha**, onde os funcionários anotam e executam o preparo. | pelo AI Waiter | com o restaurante |
| 3 | **Assistente + Pedidos + Pagamento** | Tudo da 2, e o cliente paga por **Pix gerado dentro do chat**. Os pagamentos são notificados na **sessão do restaurante** que acompanha a cozinha. | pelo AI Waiter | Pix no chat |

## MVP

**O MVP é apenas a modalidade 1.** Vamos começar por ela e incrementar.

Dentro do MVP:

- Cadastro da conta, do restaurante e das unidades.
- **Importação do cardápio** (PDF/imagem/planilha) com **ajustes manuais** depois:
  categorias, itens, preços, descrições, ingredientes, atributos como "apimentado" e
  "serve N pessoas". O **sem glúten** é um booleano marcado e revisado só manualmente; a
  importação nunca o marca.
- Configuração do modelo de IA padrão (ver "IA" abaixo).
- Chat do cliente na mesa, via QR code ou NFC, sem login.
- Assistente que responde só sobre o cardápio, monta combos por orçamento e faz sugestões
  de acompanhamento.
- Landing page com lista de espera (já existe).

Fora do MVP (modalidades 2 e 3):

- Pedido pelo chat e sistema da cozinha.
- Pix no chat e notificação de pagamento na sessão do restaurante.
- Reservas com mapa do salão, perfis de clientes, campanhas, integração com WhatsApp
  (prometidos na landing, mas depois do MVP).

## Conta, restaurante e unidade

Hierarquia: **Conta → Restaurante → Unidade**. A cobrança é **por unidade**. Cada unidade
tem seu próprio cardápio (pode ser copiado de outra unidade) e suas próprias mesas.
O plano gratuito cobre 1 unidade.

## Cardápio: importação + ajustes manuais

O caminho principal é a **importação**: o restaurante envia o cardápio (PDF, foto,
planilha) e o sistema extrai categorias, itens, preços, descrições e atributos usando IA.
Em seguida o restaurante **revisa e ajusta manualmente** o que quiser (corrigir preço,
marcar "sem glúten", apagar item). O cadastro 100% manual também é possível, mas é o
caminho secundário. Meta: "do PDF à mesa em cerca de 20 minutos".

## IA

- **Provedor principal: OpenAI.** **Fallback: Anthropic.** Se a chamada ao principal
  falhar (erro, timeout, rate limit), o sistema tenta o fallback automaticamente.
- **O modelo padrão é configurável via banco**, não hardcoded: uma tabela de configuração
  guarda provedor e modelo default (e, se precisar, override por restaurante). Trocar o
  modelo não exige deploy.
- No código, o assistente fica atrás de uma abstração única (`IWaiterAssistant` em
  Application); OpenAI e Anthropic são duas implementações em Infrastructure, e um
  decorator/orquestrador faz a escolha e o fallback.
- O assistente é restrito ao cardápio da unidade: o cardápio entra como contexto do
  modelo e o prompt de sistema proíbe sair do assunto. O prompt sozinho não basta: os
  guardrails (anti-alucinação, prompt injection, combos, escopo, idioma) são em camadas,
  com validação em código, e estão em [guardrails.md](guardrails.md).

## Glossário de domínio (nomes para o código)

| Termo | Nome no código | Significado |
|---|---|---|
| Conta | `Account` | Cliente pagante (dono/rede). Possui restaurantes. |
| Restaurante | `Restaurant` | Marca/estabelecimento dentro de uma conta. Possui unidades. |
| Unidade | `Unit` | Local físico. **É a unidade de cobrança.** Possui cardápio e mesas. |
| Cardápio | `Menu` | Conjunto de categorias e itens de uma unidade. |
| Importação de cardápio | `MenuImport` | Processo de extrair o cardápio de um arquivo enviado, para revisão manual. |
| Config. de IA | `AiModelSettings` | Provedor e modelo default (e override por restaurante), lidos do banco. |
| Categoria | `MenuCategory` | Agrupamento de itens (pizzas, vinhos, sobremesas). |
| Item | `MenuItem` | Prato/bebida com preço, descrição, ingredientes, atributos (apimentado, serve N). `IsGlutenFree` é booleano marcado só manualmente. |
| Mesa | `Table` | Ponto de acesso físico (QR/NFC) que identifica onde o cliente está. |
| Conversa | `Conversation` | Sessão de chat de um cliente numa mesa. Guarda o idioma (`Language`) detectado na primeira mensagem. |
| AI Waiter | `Waiter` / `Assistant` | O agente de IA, restrito ao cardápio do restaurante. |
| Combo | `ComboSuggestion` | Conjunto de itens montado pelo assistente dentro de um orçamento (no máximo 20% acima). |
| Sugestão | `Upsell` | Acompanhamento oferecido em função do que o cliente escolheu. |
| Pedido | `Order` | Só na modalidade 2+. |
| Pagamento | `Payment` | Só na modalidade 3. |

## Decisões tomadas

| Data | Decisão |
|---|---|
| 2026-11-15 | Nome definitivo: **Meu Garçom**. |
| 2026-11-15 | Cobrança **por unidade**; hierarquia Conta → Restaurante → Unidade. |
| 2026-11-15 | Cardápio entra por **importação** com **ajustes manuais** depois. |
| 2026-11-15 | IA: **OpenAI** principal, **Anthropic** fallback, modelo default **configurável via banco**. |
| 2026-11-18 | O assistente só informa o que está **explícito no cardápio**: nada inferido pelo nome do prato nem pela receita tradicional. |
| 2026-11-18 | **Sem glúten** é um booleano (`IsGlutenFree`) marcado e revisado só manualmente; a importação nunca o marca. |
| 2026-11-18 | Prompt injection e jailbreak: seguir obrigatoriamente o **OWASP Top 10 for LLM Applications** e os guias dos provedores. |
| 2026-11-18 | Combo: se não fechar no orçamento, pode passar **no máximo 20%**, com aviso explícito ao cliente. |
| 2026-11-18 | O assistente nunca promete nada fora do escopo. |
| 2026-11-18 | Responde no **idioma da primeira mensagem** do cliente. |

## Perguntas em aberto

- Formatos aceitos na importação (PDF, imagem, planilha, link?) e se a importação usa o
  mesmo provedor de IA do chat.
- Como o cardápio é passado ao modelo: contexto direto no prompt (simples, cabe para
  cardápios pequenos) ou busca por relevância (para cardápios grandes).
- Override de modelo por restaurante é necessário no MVP ou só global?
- Combo: abaixo do orçamento, o que conta como "próximo"? (ex.: só aceitar combos que usem
  pelo menos 80% do valor antes de partir para os 20% acima.)
- Os 20% do combo ficam fixos ou são configuráveis por unidade?
- Além do sem glúten, entram outros booleanos revisados manualmente (vegano, sem lactose)?
- Quais idiomas o catálogo de respostas padrão cobre (PT, EN, ES?), e o que fazer se o
  cliente trocar de idioma no meio da conversa?
- Valores dos limites de uso: tamanho da mensagem, rate limit, mensagens por conversa.
