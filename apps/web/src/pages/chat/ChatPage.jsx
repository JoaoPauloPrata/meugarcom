import { useEffect, useRef, useState } from 'react'
import { useParams } from 'react-router-dom'
import { publicApi } from '../../lib/api'
import { DIETARY_LABELS, formatBRL, greetingForNow, itemTags } from '../../lib/format'
import Icon from '../../components/Icon'
import { Dialog, Spinner } from '../../components/ui'

const MAX_LENGTH = 500

export default function ChatPage() {
  const { code } = useParams()
  const [session, setSession] = useState(null) // { conversationId, restaurantName, tableNumber, suggestions }
  const [startError, setStartError] = useState('')
  const [messages, setMessages] = useState([])
  const [text, setText] = useState('')
  const [sending, setSending] = useState(false)
  const [closed, setClosed] = useState(false)
  const [menuOpen, setMenuOpen] = useState(false)
  const startedRef = useRef(false)
  const scrollRef = useRef(null)
  const inputRef = useRef(null)

  // A new conversation every time the chat is opened (no persistence between visits).
  useEffect(() => {
    if (startedRef.current) return
    startedRef.current = true
    publicApi
      .startConversation(code)
      .then((started) => {
        setSession(started)
        document.title = `${started.restaurantName} · Meu Garçom`
      })
      .catch((e) => setStartError(e.message))
  }, [code])

  // While the user is asking (or the waiter is typing) follow the bottom; once a reply arrives,
  // pin its start to the top so long lists are read from the first item down.
  useEffect(() => {
    const node = scrollRef.current
    if (!node) return
    const behavior = window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth'
    const last = messages[messages.length - 1]
    const lastNode = !sending && last && last.role !== 'user' ? node.querySelector(`[data-message-id="${last.id}"]`) : null
    if (!lastNode) {
      node.scrollTo({ top: node.scrollHeight, behavior })
      return
    }
    const offset = lastNode.getBoundingClientRect().top - node.getBoundingClientRect().top
    node.scrollTo({ top: node.scrollTop + offset - 12, behavior })
  }, [messages, sending])

  async function send(content) {
    const value = content.trim()
    if (!value || sending || closed || !session) return
    setText('')
    setMessages((list) => [...list, { id: crypto.randomUUID(), role: 'user', text: value }])
    setSending(true)
    try {
      const reply = await publicApi.sendMessage(session.conversationId, value)
      setMessages((list) => [
        ...list,
        {
          id: crypto.randomUUID(),
          role: 'bot',
          text: reply.reply,
          items: reply.items,
          combo: reply.combo,
          quickReplies: reply.quickReplies ?? [],
        },
      ])
      if (reply.conversationClosed) setClosed(true)
    } catch (e) {
      setMessages((list) => [...list, { id: crypto.randomUUID(), role: 'error', text: e.message }])
      if (e.code === 'conversation.closed' || e.code === 'conversation.expired' || e.code === 'conversation.message_limit_reached') {
        setClosed(true)
      }
    } finally {
      setSending(false)
      inputRef.current?.focus()
    }
  }

  function onKeyDown(e) {
    if (e.key !== 'Enter' || e.shiftKey || e.ctrlKey || e.metaKey || e.altKey) return
    e.preventDefault()
    // IME composition (e.g. Japanese, Korean) uses Enter to confirm text, not to send.
    if (e.nativeEvent.isComposing || e.keyCode === 229) return
    if (!e.repeat) send(text)
  }

  function autoGrow(node) {
    if (!node) return
    node.style.height = 'auto'
    node.style.height = `${Math.min(node.scrollHeight, 120)}px`
  }

  if (startError) {
    return (
      <div className="chat-page">
        <div className="chat-app">
          <div className="chat-state">
            <img src="/logo-icon.png" alt="" width="96" height="39" />
            <h2>Ops!</h2>
            <p>{startError}</p>
          </div>
        </div>
      </div>
    )
  }

  if (!session) {
    return (
      <div className="chat-page">
        <div className="chat-app">
          <div className="chat-state" role="status">
            <Spinner label="Abrindo o chat" />
          </div>
        </div>
      </div>
    )
  }

  const subtitle = [session.restaurantName, session.tableNumber ? `Mesa ${session.tableNumber}` : null].filter(Boolean).join(' · ')

  return (
    <div className="chat-page">
      <div className="chat-app">
        <header className="chat-header">
          <span className="chat-avatar" aria-hidden="true">
            <img src="/logo-icon.png" alt="" width="30" height="12" />
          </span>
          <div>
            <h1>Meu Garçom</h1>
            <p>{subtitle}</p>
          </div>
          <button type="button" className="icon-btn icon-btn--plain" onClick={() => setMenuOpen(true)} aria-label="Ver cardápio completo">
            <Icon name="book" />
          </button>
        </header>

        <div className="chat-scroll" ref={scrollRef}>
          {messages.length === 0 && (
            <div className="chat-welcome">
              <img src="/logo-icon.png" alt="" width="96" height="39" />
              <h2>{greetingForNow()}! O que vamos pedir hoje?</h2>
              <div className="suggestions">
                {(session.suggestions ?? []).map((s) => (
                  <button key={s.text} type="button" className="suggestion" onClick={() => send(s.text)} disabled={sending}>
                    <Icon name={s.icon} size={24} />
                    {s.text}
                  </button>
                ))}
                {/* Opening the full menu is an app action, not a question, so it is always offered. */}
                <button type="button" className="suggestion" onClick={() => setMenuOpen(true)}>
                  <Icon name="book" size={24} />
                  Ver cardápio completo
                </button>
              </div>
            </div>
          )}

          <div className="chat-log" role="log" aria-live="polite" aria-relevant="additions">
            {messages.map((m, index) =>
              m.role === 'user' ? (
                <p key={m.id} className="bubble bubble--user">
                  <span className="sr-only">Você: </span>
                  {m.text}
                </p>
              ) : m.role === 'error' ? (
                <p key={m.id} data-message-id={m.id} className="bubble bubble--error" role="alert">
                  {m.text}
                </p>
              ) : (
                <div key={m.id} data-message-id={m.id} className="bot-group">
                  <div className="bot-row">
                    <BotAvatar />
                    <p className="bubble bubble--bot">
                      <span className="sr-only">Garçom: </span>
                      {m.text}
                    </p>
                  </div>
                  {m.combo && <ComboCard combo={m.combo} />}
                  {m.items?.length > 0 && !m.combo && (
                    <div className="dish-cards">
                      {m.items.map((item) => (
                        <DishCard key={item.id} item={item} tags={item.tags} />
                      ))}
                    </div>
                  )}
                  {index === messages.length - 1 && !sending && !closed && m.quickReplies?.length > 0 && (
                    <QuickReplies options={m.quickReplies} onPick={send} />
                  )}
                </div>
              ),
            )}
          </div>
          {sending && (
            <div className="bot-row">
              <BotAvatar />
              <div className="typing-dots" role="status" aria-label="O garçom está digitando">
                <span />
                <span />
                <span />
              </div>
            </div>
          )}
          {closed && <p className="bubble bubble--system">Conversa encerrada. Se precisar, chame o garçom.</p>}
        </div>

        <form
          className="chat-composer"
          onSubmit={(e) => {
            e.preventDefault()
            send(text)
          }}
        >
          <div className="composer-field">
            <label htmlFor="chat-input" className="sr-only">
              Pergunte ao garçom
            </label>
            <textarea
              id="chat-input"
              ref={(node) => {
                inputRef.current = node
                autoGrow(node)
              }}
              rows={1}
              value={text}
              maxLength={MAX_LENGTH}
              placeholder={closed ? 'Conversa encerrada' : 'Pergunte ao garçom…'}
              disabled={closed}
              enterKeyHint="send"
              onChange={(e) => {
                setText(e.target.value)
                autoGrow(e.target)
              }}
              onKeyDown={onKeyDown}
            />
            {text.length > MAX_LENGTH - 80 && (
              <span className="composer-count" aria-live="polite">
                {text.length}/{MAX_LENGTH}
              </span>
            )}
          </div>
          <button type="submit" className="send-btn" disabled={!text.trim() || sending || closed} aria-label="Enviar pergunta">
            {sending ? <Spinner label="Enviando" /> : <Icon name="send" size={20} />}
          </button>
        </form>
        <p className="chat-footnote">O garçom de IA responde com base no cardápio. Na dúvida, confirme com a equipe.</p>
      </div>

      {menuOpen && <MenuSheet code={code} onClose={() => setMenuOpen(false)} />}
    </div>
  )
}

function BotAvatar() {
  return (
    <span className="bot-avatar" aria-hidden="true">
      <img src="/logo-icon.png" alt="" width="22" height="9" />
    </span>
  )
}

function QuickReplies({ options, onPick }) {
  return (
    <ul className="quick-replies" aria-label="Sugestões de resposta">
      {options.map((option) => (
        <li key={option}>
          <button type="button" className="quick-reply" onClick={() => onPick(option)}>
            {option}
          </button>
        </li>
      ))}
    </ul>
  )
}

function DishCard({ item, tags }) {
  const [imageFailed, setImageFailed] = useState(false)
  const showImage = item.imageUrl && !imageFailed
  return (
    <article className={`dish-card ${showImage ? 'dish-card--photo' : ''}`}>
      {showImage && (
        <img
          className="dish-card-photo"
          src={item.imageUrl}
          alt=""
          width="72"
          height="72"
          loading="lazy"
          decoding="async"
          onError={() => setImageFailed(true)}
        />
      )}
      <div className="dish-card-top">
        <h3>{item.name}</h3>
        <span className="dish-price">{formatBRL(item.price)}</span>
      </div>
      {item.description && <p>{item.description}</p>}
      {(tags.length > 0 || item.portion || item.servesPeople) && (
        <div className="tag-list">
          {tags.map((tag) => (
            <span key={tag} className={`diet-tag ${tag === 'spicy' ? 'diet-tag--spicy' : ''}`}>
              {DIETARY_LABELS[tag]}
            </span>
          ))}
          {item.portion && <span className="diet-tag diet-tag--muted">{item.portion}</span>}
          {item.servesPeople && (
            <span className="diet-tag diet-tag--muted">
              Serve {item.servesPeople} {item.servesPeople === 1 ? 'pessoa' : 'pessoas'}
            </span>
          )}
        </div>
      )}
    </article>
  )
}

function ComboLine({ line }) {
  const [imageFailed, setImageFailed] = useState(false)
  const showImage = line.imageUrl && !imageFailed
  return (
    <li>
      <span className="combo-line-name">
        {showImage && (
          <img
            className="combo-line-photo"
            src={line.imageUrl}
            alt=""
            width="44"
            height="44"
            loading="lazy"
            decoding="async"
            onError={() => setImageFailed(true)}
          />
        )}
        <span>
          {line.quantity}× {line.name}
        </span>
      </span>
      <span className="combo-line-price">{formatBRL(line.subtotal)}</span>
    </li>
  )
}

function ComboCard({ combo }) {
  return (
    <section className="combo-card" aria-label="Combo sugerido">
      <h3>Combo sugerido · orçamento {formatBRL(combo.budget)}</h3>
      <ul className="combo-lines">
        {combo.lines.map((line) => (
          <ComboLine key={line.itemId} line={line} />
        ))}
      </ul>
      <div className="combo-total">
        <span>Total</span>
        <span>{formatBRL(combo.total)}</span>
      </div>
      {combo.notice && <p className="combo-notice">{combo.notice}</p>}
    </section>
  )
}

// Case- and accent-insensitive, so "acai" finds "Açaí".
function normalizeSearch(value) {
  return String(value ?? '')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim()
}

function MenuSheet({ code, onClose }) {
  const [menu, setMenu] = useState(null)
  const [error, setError] = useState('')
  const [query, setQuery] = useState('')
  const [categoryId, setCategoryId] = useState(null) // null = all categories
  const filtersRef = useRef(null)

  useEffect(() => {
    publicApi
      .getMenu(code)
      .then(setMenu)
      .catch((e) => setError(e.message))
  }, [code])

  const categories = (menu?.categories ?? []).filter((category) => category.items.length > 0)
  const term = normalizeSearch(query)
  const visible = categories
    .filter((category) => categoryId === null || category.id === categoryId)
    .map((category) => ({
      ...category,
      items: term
        ? category.items.filter((item) => normalizeSearch(`${item.name} ${item.description ?? ''}`).includes(term))
        : category.items,
    }))
    .filter((category) => category.items.length > 0)
  const resultCount = visible.reduce((sum, category) => sum + category.items.length, 0)
  const filtering = term !== '' || categoryId !== null

  // Back to the top of the list when the category changes, so results start right under the filters.
  useEffect(() => {
    filtersRef.current?.closest('.panel-body')?.scrollTo({ top: 0 })
  }, [categoryId])

  function clearFilters() {
    setQuery('')
    setCategoryId(null)
  }

  return (
    <Dialog title="Cardápio" variant="sheet" onClose={onClose}>
      {error && <p className="bubble bubble--error">{error}</p>}
      {!menu && !error && <Spinner label="Carregando cardápio" />}
      {menu && categories.length === 0 && <p>O cardápio ainda não foi publicado.</p>}
      {categories.length > 0 && (
        <>
          <div className="menu-filters" ref={filtersRef}>
            <div className="menu-search">
              <Icon name="search" size={18} />
              <label htmlFor="menu-search-input" className="sr-only">
                Buscar no cardápio
              </label>
              <input
                id="menu-search-input"
                type="search"
                className="input"
                placeholder="Buscar prato ou ingrediente"
                value={query}
                enterKeyHint="search"
                autoComplete="off"
                onChange={(e) => setQuery(e.target.value)}
              />
              {query && (
                <button type="button" className="menu-search-clear" onClick={() => setQuery('')} aria-label="Limpar busca">
                  <Icon name="close" size={16} />
                </button>
              )}
            </div>
            {categories.length > 1 && (
              <div className="menu-chips" role="group" aria-label="Filtrar por categoria">
                <button
                  type="button"
                  className="menu-chip"
                  aria-pressed={categoryId === null}
                  onClick={() => setCategoryId(null)}
                >
                  Todos
                </button>
                {categories.map((category) => (
                  <button
                    key={category.id}
                    type="button"
                    className="menu-chip"
                    aria-pressed={categoryId === category.id}
                    onClick={() => setCategoryId(category.id)}
                  >
                    {category.name}
                  </button>
                ))}
              </div>
            )}
          </div>

          <p className="sr-only" aria-live="polite">
            {filtering ? `${resultCount} ${resultCount === 1 ? 'item encontrado' : 'itens encontrados'}` : ''}
          </p>

          {visible.length === 0 ? (
            <div className="menu-empty">
              <p>
                Nada encontrado{term ? ` para “${query.trim()}”` : ''}. Tente outra palavra ou veja todas as categorias.
              </p>
              <button type="button" className="btn btn--ghost" onClick={clearFilters}>
                Limpar filtros
              </button>
            </div>
          ) : (
            visible.map((category) => (
              <section key={category.id} className="sheet-category">
                <h3>{category.name}</h3>
                <div className="dish-cards">
                  {category.items.map((item) => (
                    <DishCard key={item.id} item={item} tags={itemTags(item)} />
                  ))}
                </div>
              </section>
            ))
          )}
        </>
      )}
    </Dialog>
  )
}
