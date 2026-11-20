import { useEffect, useRef, useState } from 'react'
import { useParams } from 'react-router-dom'
import { publicApi } from '../../lib/api'
import { DIETARY_LABELS, formatBRL, greetingForNow, itemTags } from '../../lib/format'
import Icon from '../../components/Icon'
import { Dialog, Spinner } from '../../components/ui'

const MAX_LENGTH = 500

const SUGGESTIONS = [
  { icon: 'chat', label: 'O que você recomenda?', prompt: 'O que você recomenda?' },
  { icon: 'wheatOff', label: 'Opções sem glúten', prompt: 'Quais opções são sem glúten?' },
  { icon: 'wine', label: 'Harmonizar com vinho', prompt: 'Que vinho combina com os pratos principais?' },
  { icon: 'book', label: 'Ver cardápio completo', action: 'menu' },
]

export default function ChatPage() {
  const { code } = useParams()
  const [session, setSession] = useState(null) // { conversationId, restaurantName, tableNumber }
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

  useEffect(() => {
    const node = scrollRef.current
    if (node) node.scrollTo({ top: node.scrollHeight, behavior: 'smooth' })
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
                {SUGGESTIONS.map((s) => (
                  <button
                    key={s.label}
                    type="button"
                    className="suggestion"
                    onClick={() => (s.action === 'menu' ? setMenuOpen(true) : send(s.prompt))}
                    disabled={sending}
                  >
                    <Icon name={s.icon} size={24} />
                    {s.label}
                  </button>
                ))}
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
                <p key={m.id} className="bubble bubble--error" role="alert">
                  {m.text}
                </p>
              ) : (
                <div key={m.id} className="bot-group">
                  <p className="bubble bubble--bot">
                    <span className="sr-only">Garçom: </span>
                    {m.text}
                  </p>
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
            <div className="typing-dots" role="status" aria-label="O garçom está digitando">
              <span />
              <span />
              <span />
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
  return (
    <article className="dish-card">
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

function ComboCard({ combo }) {
  return (
    <section className="combo-card" aria-label="Combo sugerido">
      <h3>Combo sugerido · orçamento {formatBRL(combo.budget)}</h3>
      <ul className="combo-lines">
        {combo.lines.map((line) => (
          <li key={line.itemId}>
            <span>
              {line.quantity}× {line.name}
            </span>
            <span>{formatBRL(line.subtotal)}</span>
          </li>
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

function MenuSheet({ code, onClose }) {
  const [menu, setMenu] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    publicApi
      .getMenu(code)
      .then(setMenu)
      .catch((e) => setError(e.message))
  }, [code])

  return (
    <Dialog title="Cardápio" variant="sheet" onClose={onClose}>
      {error && <p className="bubble bubble--error">{error}</p>}
      {!menu && !error && <Spinner label="Carregando cardápio" />}
      {menu?.categories.length === 0 && <p>O cardápio ainda não foi publicado.</p>}
      {menu?.categories.map((category) => (
        <section key={category.id} className="sheet-category">
          <h3>{category.name}</h3>
          <div className="dish-cards">
            {category.items.map((item) => (
              <DishCard key={item.id} item={item} tags={itemTags(item)} />
            ))}
          </div>
        </section>
      ))}
    </Dialog>
  )
}
