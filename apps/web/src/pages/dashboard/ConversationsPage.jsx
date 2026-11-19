import { useEffect, useState } from 'react'
import { useOutletContext } from 'react-router-dom'
import { api } from '../../lib/api'
import { Alert, Spinner } from '../../components/ui'

const dateTime = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

const OUTCOME_LABELS = {
  Blocked: 'Bloqueada pelo filtro de segurança',
  Abuse: 'Mensagem ofensiva',
  SafeFallback: 'Resposta segura padrão',
}

export default function ConversationsPage() {
  const { unit } = useOutletContext()
  const [conversations, setConversations] = useState(null)
  const [selectedId, setSelectedId] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    api
      .get(`/api/units/${unit.id}/conversations`)
      .then((list) => {
        if (!active) return
        setConversations(list)
        setSelectedId(list[0]?.id ?? null)
      })
      .catch((e) => active && setError(e.message))
    return () => {
      active = false
    }
  }, [unit.id])

  const selected = conversations?.find((c) => c.id === selectedId)

  return (
    <>
      <div className="page-head">
        <div>
          <h1>Conversas</h1>
          <p>O que os clientes estão perguntando ao garçom de IA. Somente leitura.</p>
        </div>
      </div>
      <Alert>{error}</Alert>
      {!conversations && !error && <Spinner label="Carregando conversas" />}
      {conversations?.length === 0 && (
        <div className="card empty">
          <h2>Nenhuma conversa ainda</h2>
          <p>Quando os clientes escanearem o QR code e fizerem perguntas, as conversas aparecem aqui.</p>
        </div>
      )}
      {conversations?.length > 0 && (
        <div className="convo-layout">
          <ul className="convo-list" aria-label="Conversas recentes">
            {conversations.map((c) => {
              const firstQuestion = c.messages.find((m) => m.role === 'customer')?.content ?? ''
              return (
                <li key={c.id}>
                  <button type="button" aria-current={c.id === selectedId ? 'true' : undefined} onClick={() => setSelectedId(c.id)}>
                    <span className="convo-title">
                      <span>{c.tableNumber ? `Mesa ${c.tableNumber}` : 'Link geral'}</span>
                      <small style={{ color: 'var(--muted)', fontWeight: 400 }}>{dateTime.format(new Date(c.startedAt))}</small>
                    </span>
                    <span className="convo-preview">{firstQuestion}</span>
                  </button>
                </li>
              )
            })}
          </ul>
          {selected && (
            <section className="card convo-thread" aria-label="Mensagens da conversa">
              {selected.messages.map((m, i) => (
                <div key={i} className={`bubble ${m.role === 'customer' ? 'bubble--user' : 'bubble--bot'}`} style={{ boxShadow: 'none', border: m.role === 'customer' ? 0 : '1px solid var(--line)' }}>
                  {m.content}
                  {OUTCOME_LABELS[m.outcome] && (
                    <span className="diet-tag diet-tag--muted" style={{ display: 'flex', width: 'fit-content', marginTop: 6 }}>
                      {OUTCOME_LABELS[m.outcome]}
                    </span>
                  )}
                </div>
              ))}
            </section>
          )}
        </div>
      )}
    </>
  )
}
