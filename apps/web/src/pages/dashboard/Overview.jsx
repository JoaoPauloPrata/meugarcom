import { useEffect, useState } from 'react'
import { Link, useOutletContext } from 'react-router-dom'
import { api } from '../../lib/api'
import Icon from '../../components/Icon'
import { Alert, Spinner } from '../../components/ui'

const DIETARY = {
  lactose_free: 'Sem lactose',
  gluten_free: 'Sem glúten',
  vegan: 'Vegano',
}

export default function Overview() {
  const { unit } = useOutletContext()
  const [data, setData] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    api
      .get(`/api/units/${unit.id}/overview`)
      .then((overview) => active && setData(overview))
      .catch((e) => active && setError(e.message))
    return () => {
      active = false
    }
  }, [unit.id])

  return (
    <>
      <div className="page-head">
        <div>
          <h1>Visão geral</h1>
          <p>{unit.name}</p>
        </div>
      </div>
      <Alert>{error}</Alert>
      {!data && !error && <Spinner label="Carregando indicadores" />}
      {data && (
        <>
          {(data.menuItems === 0 || data.tables === 0 || data.questionsToday === 0) && (
            <GettingStarted data={data} unit={unit} />
          )}
          <div className="kpis">
            <Kpi label="Conversas hoje" value={data.conversationsToday} />
            <Kpi label="Perguntas ao garçom hoje" value={data.questionsToday} />
            <Kpi label="Itens no cardápio" value={data.menuItems} />
            <Kpi label="Mesas" value={data.tables} />
          </div>
          <div className="overview-grid">
            <section className="card" aria-labelledby="by-hour-title">
              <h2 id="by-hour-title">Perguntas por hora (últimos 7 dias)</h2>
              <QuestionsByHour values={data.questionsByHour} />
            </section>
            <section className="card" aria-labelledby="dietary-title">
              <h2 id="dietary-title">Restrições mais perguntadas (30 dias)</h2>
              <DietaryBars items={data.dietaryQuestions} />
              <p className="chart-caption">
                <Icon name="shield" size={14} /> {data.blockedAttemptsLast7Days} mensagens bloqueadas pelos filtros de segurança
                nos últimos 7 dias.
              </p>
            </section>
          </div>
        </>
      )}
    </>
  )
}

function Kpi({ label, value }) {
  return (
    <div className="kpi">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

function GettingStarted({ data, unit }) {
  const steps = [
    { done: data.menuItems > 0, label: 'Cadastre ou importe o cardápio', to: '/dashboard/menu', cta: 'Ir ao cardápio' },
    { done: data.tables > 0, label: 'Gere os QR codes das mesas', to: '/dashboard/tables', cta: 'Ver mesas' },
    { done: data.questionsToday > 0, label: 'Faça uma pergunta de teste ao garçom', href: `/m/${unit.publicCode}`, cta: 'Abrir o chat' },
  ]
  return (
    <section className="card" style={{ marginBottom: 20 }} aria-labelledby="start-title">
      <h2 id="start-title">Comece por aqui</h2>
      <ol className="checklist">
        {steps.map((s) => (
          <li key={s.label} className={s.done ? 'is-done' : ''}>
            <span className="check-dot">
              <Icon name="check" size={14} strokeWidth={2.4} />
            </span>
            <span>
              {s.label}
              {s.done && <span className="sr-only"> (concluído)</span>}
            </span>
            {s.href ? (
              <a href={s.href} target="_blank" rel="noreferrer">
                {s.cta}
              </a>
            ) : (
              <Link to={s.to}>{s.cta}</Link>
            )}
          </li>
        ))}
      </ol>
    </section>
  )
}

function QuestionsByHour({ values }) {
  const [active, setActive] = useState(null)
  const max = Math.max(...values, 0)
  const total = values.reduce((a, b) => a + b, 0)

  if (total === 0) {
    return <p className="chart-caption">Ainda não há perguntas nos últimos 7 dias.</p>
  }

  return (
    <>
      <div className="bar-chart" onMouseLeave={() => setActive(null)}>
        {values.map((count, hour) => {
          const label = `${hour}h: ${count} ${count === 1 ? 'pergunta' : 'perguntas'}`
          return (
            <div
              key={hour}
              className="bar-col"
              tabIndex={0}
              aria-label={label}
              onMouseEnter={() => setActive(hour)}
              onFocus={() => setActive(hour)}
              onBlur={() => setActive(null)}
            >
              {active === hour && (
                <span className="bar-tip" aria-hidden="true">
                  {label}
                </span>
              )}
              <span className="hour-bar" style={{ height: max ? `${(count / max) * 100}%` : 0 }} aria-hidden="true" />
              <span className="bar-label" aria-hidden="true">
                {hour}
              </span>
            </div>
          )
        })}
      </div>
      <details className="chart-table">
        <summary>Ver tabela</summary>
        <table>
          <thead>
            <tr>
              <th scope="col">Hora</th>
              <th scope="col">Perguntas</th>
            </tr>
          </thead>
          <tbody>
            {values.map((count, hour) =>
              count > 0 ? (
                <tr key={hour}>
                  <td>{hour}h</td>
                  <td>{count}</td>
                </tr>
              ) : null,
            )}
          </tbody>
        </table>
      </details>
    </>
  )
}

function DietaryBars({ items }) {
  const max = Math.max(...items.map((i) => i.count), 0)
  return (
    <div className="hbar-list">
      {items.map((item) => (
        <div key={item.attribute} className="hbar-row">
          <span>{DIETARY[item.attribute] ?? item.attribute}</span>
          <div className="hbar-track" aria-hidden="true">
            <div className="hbar-fill" style={{ width: max ? `${(item.count / max) * 100}%` : 0 }} />
          </div>
          <strong>{item.count}</strong>
        </div>
      ))}
    </div>
  )
}
