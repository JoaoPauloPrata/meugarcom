import { useEffect, useRef, useState } from 'react'

/* ------------------------------------------------------------------ */
/* Dados do roteiro                                                    */
/* As imagens são opcionais: coloque os arquivos em /public/pratos/    */
/* ------------------------------------------------------------------ */

const DISHES = {
  palmito: { name: 'Salada de palmito', price: 'R$ 38', tag: 'Sem lactose', img: '/pratos/salada-palmito.jpg' },
  peixe: { name: 'Peixe grelhado', price: 'R$ 64', tag: 'Sem lactose', img: '/pratos/peixe-grelhado.jpg' },
  caju: { name: 'Suco de caju', price: 'R$ 14', note: 'fruta da estação', img: '/pratos/suco-caju.jpg' },
  bolinho: { name: 'Bolinho de bacalhau', price: 'R$ 32', note: '6 unidades', img: '/pratos/bolinho-bacalhau.jpg' },
  picanha: { name: 'Picanha na chapa', price: 'R$ 89', note: 'serve 2', img: '/pratos/picanha.jpg' },
  caipirinha: { name: 'Caipirinha de limão', price: 'R$ 24', note: '2 unidades', img: '/pratos/caipirinha.jpg' },
}

const CHIPS = ['O que é bom pra dividir?', 'Algo leve, sem lactose', 'A moqueca é apimentada?']

const BASE_QUESTIONS = [
  { q: 'A moqueca é apimentada?', meta: 'Mesa 4 · há 3 min', tag: 'Pimenta' },
  { q: 'Tem algo sem castanha?', meta: 'Mesa 9 · há 6 min', tag: 'Alergia' },
]

const INITIAL_DASH = { count: 128, pct: 31, questions: BASE_QUESTIONS }

/* Estado final estático (usado com prefers-reduced-motion) */
const STATIC_STATE = {
  messages: [
    { id: 's1', kind: 'user', text: 'Algo leve, sem lactose' },
    { id: 's2', kind: 'ai', text: 'Duas boas pedidas hoje. As duas são sem lactose.' },
    { id: 's3', kind: 'dish', dish: 'palmito', added: true },
    { id: 's4', kind: 'dish', dish: 'peixe', added: false },
    { id: 's5', kind: 'ai', text: 'Anotado na sua mesa. Um suco de caju vai bem com ela.' },
    { id: 's6', kind: 'dish', dish: 'caju', added: true },
  ],
  table: 2,
  input: '',
  dash: {
    count: 129,
    pct: 32,
    questions: [{ q: 'Algo leve, sem lactose', meta: 'Mesa 12 · agora', tag: 'Sem lactose', fresh: true }, ...BASE_QUESTIONS],
  },
}

let uid = 0
const nextId = () => `m${++uid}`

/* ------------------------------------------------------------------ */

function usePrefersReducedMotion() {
  const [reduced, setReduced] = useState(
    () => typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches,
  )
  useEffect(() => {
    const mq = window.matchMedia('(prefers-reduced-motion: reduce)')
    const onChange = () => setReduced(mq.matches)
    mq.addEventListener('change', onChange)
    return () => mq.removeEventListener('change', onChange)
  }, [])
  return reduced
}

function DishThumb({ dish }) {
  const [failed, setFailed] = useState(false)
  if (!dish.img || failed) {
    return (
      <span className="dish-thumb dish-thumb--placeholder" aria-hidden="true">
        {dish.name.charAt(0)}
      </span>
    )
  }
  return <img className="dish-thumb" src={dish.img} alt="" onError={() => setFailed(true)} />
}

function LeafIcon() {
  return (
    <svg width="10" height="10" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path d="M5 19c0-8 5-14 15-14 0 10-6 15-14 15M5 19l7-7" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" />
    </svg>
  )
}

function BellAvatar({ small }) {
  return (
    <span className={`bell ${small ? 'bell--sm' : ''}`} aria-hidden="true">
      <img src="/logo-icon-dark.png" alt="" />
    </span>
  )
}

function Message({ m }) {
  if (m.kind === 'user') return <div className="msg msg--user">{m.text}</div>
  if (m.kind === 'typing')
    return (
      <div className="msg-row">
        <BellAvatar small />
        <div className="typing" aria-label="digitando">
          <i /> <i /> <i />
        </div>
      </div>
    )
  if (m.kind === 'ai')
    return (
      <div className="msg-row">
        <BellAvatar small />
        <p className="msg msg--ai">{m.text}</p>
      </div>
    )
  if (m.kind === 'chips')
    return (
      <div className="chips">
        {CHIPS.map((c) => (
          <span key={c} className={`chip ${m.active === c ? 'chip--active' : ''}`}>
            {c}
          </span>
        ))}
      </div>
    )
  if (m.kind === 'dish') {
    const d = DISHES[m.dish]
    return (
      <div className="dish">
        <DishThumb dish={d} />
        <div className="dish-info">
          <strong>{d.name}</strong>
          <span>
            {d.price}
            {d.tag ? (
              <em className="tag tag--ok">
                <LeafIcon /> {d.tag}
              </em>
            ) : (
              <> · {d.note}</>
            )}
          </span>
        </div>
        <span className={`add-btn ${m.added ? 'add-btn--done' : ''}`}>
          {m.added ? (
            <>
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                <path d="M5 12.5l4.5 4.5L19 7.5" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
              Adicionado
            </>
          ) : (
            <span aria-label="adicionar">+</span>
          )}
        </span>
      </div>
    )
  }
  return null
}

/* ------------------------------------------------------------------ */

export default function Demo() {
  const reduced = usePrefersReducedMotion()
  const [messages, setMessages] = useState(STATIC_STATE.messages)
  const [table, setTable] = useState(STATIC_STATE.table)
  const [input, setInput] = useState('')
  const [dash, setDash] = useState(STATIC_STATE.dash)
  const [paused, setPaused] = useState(false)
  const [visible, setVisible] = useState(false)

  const rootRef = useRef(null)
  const chatRef = useRef(null)
  const haltRef = useRef(false)

  // Pausa quando fora da tela ou quando o usuário pausa
  useEffect(() => {
    const io = new IntersectionObserver(([e]) => setVisible(e.isIntersecting), { threshold: 0.25 })
    if (rootRef.current) io.observe(rootRef.current)
    return () => io.disconnect()
  }, [])
  useEffect(() => {
    haltRef.current = paused || !visible || document.hidden
  }, [paused, visible])

  // Mantém a conversa rolada para o fim
  useEffect(() => {
    const el = chatRef.current
    if (el) el.scrollTo({ top: el.scrollHeight, behavior: reduced ? 'auto' : 'smooth' })
  }, [messages, table, reduced])

  // Roteiro da animação
  useEffect(() => {
    if (reduced) {
      setMessages(STATIC_STATE.messages)
      setTable(STATIC_STATE.table)
      setDash(STATIC_STATE.dash)
      setInput('')
      return
    }

    let cancelled = false
    const wait = (ms) =>
      new Promise((resolve, reject) => {
        let left = ms
        const tick = () => {
          if (cancelled) return reject(new Error('cancelled'))
          if (!haltRef.current && !document.hidden) left -= 50
          if (left <= 0) resolve()
          else setTimeout(tick, 50)
        }
        setTimeout(tick, 50)
      })

    const push = (m) => {
      const msg = { id: nextId(), ...m }
      setMessages((prev) => [...prev.filter((x) => x.kind !== 'typing'), msg])
      return msg.id
    }
    const patch = (id, changes) => setMessages((prev) => prev.map((x) => (x.id === id ? { ...x, ...changes } : x)))
    const ask = (question) =>
      setDash((d) => ({
        count: d.count + 1,
        pct: question.tag === 'Sem lactose' ? d.pct + 1 : d.pct,
        questions: [{ ...question, fresh: true }, ...d.questions.map((q) => ({ ...q, fresh: false }))].slice(0, 3),
      }))
    const typeInto = async (text) => {
      for (let i = 1; i <= text.length; i++) {
        setInput(text.slice(0, i))
        await wait(42)
      }
    }

    async function sceneLight() {
      setMessages([])
      setTable(0)
      setInput('')
      setDash(INITIAL_DASH)
      await wait(400)
      push({ kind: 'ai', text: 'Fome? Que bom. Conheço esse cardápio de cor. Do que você está a fim hoje?' })
      await wait(500)
      const chips = push({ kind: 'chips', active: null })
      await wait(1800)
      patch(chips, { active: 'Algo leve, sem lactose' })
      await wait(700)
      setMessages([{ id: nextId(), kind: 'user', text: 'Algo leve, sem lactose' }])
      await wait(400)
      ask({ q: 'Algo leve, sem lactose', meta: 'Mesa 12 · agora', tag: 'Sem lactose' })
      push({ kind: 'typing' })
      await wait(1300)
      push({ kind: 'ai', text: 'Duas boas pedidas hoje. As duas são sem lactose.' })
      await wait(450)
      const d1 = push({ kind: 'dish', dish: 'palmito', added: false })
      await wait(300)
      push({ kind: 'dish', dish: 'peixe', added: false })
      await wait(1400)
      patch(d1, { added: true })
      setTable(1)
      await wait(1200)
      push({ kind: 'typing' })
      await wait(1000)
      push({ kind: 'ai', text: 'Anotado na sua mesa. Um suco de caju vai bem com ela.' })
      await wait(450)
      const d3 = push({ kind: 'dish', dish: 'caju', added: false })
      await wait(1300)
      patch(d3, { added: true })
      setTable(2)
      await wait(3200)
    }

    async function sceneBudget() {
      setMessages([])
      setTable(0)
      await wait(700)
      const text = 'R$ 150 pra dois, monta um combo'
      await typeInto(text)
      await wait(500)
      setInput('')
      push({ kind: 'user', text })
      await wait(350)
      ask({ q: 'R$ 150 pra dois, monta um combo', meta: 'Mesa 7 · agora', tag: 'Orçamento' })
      push({ kind: 'typing' })
      await wait(1400)
      push({ kind: 'ai', text: 'Montei uma mesa completa por R$ 145: uma entrada, um principal pra dividir e as bebidas.' })
      await wait(450)
      const ids = []
      for (const key of ['bolinho', 'picanha', 'caipirinha']) {
        ids.push(push({ kind: 'dish', dish: key, added: false }))
        await wait(320)
      }
      await wait(900)
      for (let i = 0; i < ids.length; i++) {
        patch(ids[i], { added: true })
        setTable(i + 1)
        await wait(380)
      }
      await wait(3400)
    }

    ;(async () => {
      try {
        while (!cancelled) {
          await sceneLight()
          await sceneBudget()
        }
      } catch {
        /* animação cancelada */
      }
    })()

    return () => {
      cancelled = true
    }
  }, [reduced])

  const lastTable = useRef(1)
  if (table > 0) lastTable.current = table
  const dishCount = lastTable.current === 1 ? '1 prato' : `${lastTable.current} pratos`

  return (
    <div className="demo" ref={rootRef}>
      <div className="demo-panel" aria-hidden="true">
        <div className="demo-glow" />
      </div>

      <div className="phone" role="img" aria-label="Demonstração: cliente conversa com o garçom de IA pelo celular e adiciona pratos à mesa">
        <div className="phone-status">
          <span>9:41</span>
          <span className="island" />
          <span className="status-icons" aria-hidden="true">
            <svg width="16" height="11" viewBox="0 0 16 11"><path d="M1 10h2V7H1zm4 0h2V5H5zm4 0h2V3H9zm4 0h2V1h-2z" fill="currentColor" /></svg>
            <svg width="22" height="11" viewBox="0 0 22 11"><rect x=".5" y=".5" width="19" height="10" rx="2.5" stroke="currentColor" fill="none" /><rect x="2" y="2" width="15" height="7" rx="1.5" fill="currentColor" /></svg>
          </span>
        </div>
        <div className="phone-header">
          <BellAvatar />
          <div>
            <strong>Meu Garçom</strong>
            <span>Bistrô Linho, São Paulo</span>
          </div>
        </div>

        <div className="chat" ref={chatRef} aria-live="off">
          {messages.map((m) => (
            <div key={m.id} className="enter">
              <Message m={m} />
            </div>
          ))}
        </div>

        <div className={`table-bar ${table > 0 ? 'table-bar--on' : ''}`}>
          <span>Minha mesa</span>
          <span className="pill">{dishCount}</span>
        </div>

        <div className="phone-input">
          <span className={input ? 'typed' : ''}>{input || 'Pergunte ao garçom…'}</span>
          <span className={`send ${input ? 'send--on' : ''}`} aria-hidden="true">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none">
              <path d="M4 12l16-8-6 16-2.5-6.5z" fill="currentColor" />
            </svg>
          </span>
        </div>
      </div>

      <svg className="demo-link" viewBox="0 0 220 90" fill="none" aria-hidden="true">
        <path d="M2 86C60 86 80 20 140 12c30-4 54 2 76 6" stroke="currentColor" strokeWidth="1.4" strokeDasharray="4 5" />
      </svg>

      <div className="dash" aria-label="Painel do restaurante">
        <div className="dash-head">
          <strong>Bistrô Linho, hoje</strong>
          <span>Seu painel</span>
        </div>
        <div className="dash-stats">
          <div>
            <span className="dash-label">Perguntas respondidas</span>
            <span key={dash.count} className="dash-count">
              {dash.count}
            </span>
          </div>
          <div className="dash-bar">
            <div className="dash-bar-row">
              <span>Sem lactose</span>
              <span>{dash.pct}%</span>
            </div>
            <div className="bar">
              <i style={{ transform: `scaleX(${dash.pct / 100})` }} />
            </div>
          </div>
        </div>
        <span className="dash-label">Acabaram de perguntar</span>
        <ul className="dash-list">
          {dash.questions.map((q) => (
            <li key={q.q + q.meta} className={q.fresh ? 'fresh' : ''}>
              <div>
                <strong>{q.q}</strong>
                <span>{q.meta}</span>
              </div>
              <em className={`tag ${q.tag === 'Sem lactose' ? 'tag--ok' : ''}`}>{q.tag}</em>
            </li>
          ))}
        </ul>
      </div>

      {!reduced && (
        <button type="button" className="demo-toggle" onClick={() => setPaused((p) => !p)} aria-pressed={paused}>
          {paused ? (
            <svg width="12" height="12" viewBox="0 0 24 24" aria-hidden="true"><path d="M7 4l13 8-13 8z" fill="currentColor" /></svg>
          ) : (
            <svg width="12" height="12" viewBox="0 0 24 24" aria-hidden="true"><path d="M6 4h4v16H6zM14 4h4v16h-4z" fill="currentColor" /></svg>
          )}
          <span>{paused ? 'Continuar' : 'Pausar'} demonstração</span>
        </button>
      )}
    </div>
  )
}
