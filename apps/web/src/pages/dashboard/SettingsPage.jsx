import { useState } from 'react'
import { useOutletContext } from 'react-router-dom'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import Icon from '../../components/Icon'
import { Alert, Field, Spinner, useToast } from '../../components/ui'

const MAX_SUGGESTIONS = 3
const SUGGESTION_MAX_LENGTH = 80
const SUGGESTION_ICONS = [
  { name: 'chat', label: 'Conversa' },
  { name: 'sparkles', label: 'Destaque' },
  { name: 'heart', label: 'Favorito' },
  { name: 'wheatOff', label: 'Sem glúten' },
  { name: 'leaf', label: 'Vegetariano' },
  { name: 'wine', label: 'Vinho' },
  { name: 'cup', label: 'Bebidas' },
  { name: 'tag', label: 'Preço' },
]

export default function SettingsPage() {
  const { workspace, unit, reloadWorkspace } = useOutletContext()
  const { user } = useAuth()
  const [form, setForm] = useState({
    restaurantName: workspace.restaurantName,
    cuisineType: workspace.cuisineType ?? '',
    unitName: unit.name,
    city: unit.city ?? '',
    tolerance: Math.round(unit.comboBudgetTolerance * 100),
    suggestions: (unit.chatSuggestions ?? []).map((s) => ({ key: crypto.randomUUID(), ...s })),
  })
  const [errors, setErrors] = useState({})
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [toast, showToast] = useToast()

  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }))
  const setSuggestion = (key, patch) =>
    setForm((f) => ({ ...f, suggestions: f.suggestions.map((s) => (s.key === key ? { ...s, ...patch } : s)) }))
  const addSuggestion = () =>
    setForm((f) => ({ ...f, suggestions: [...f.suggestions, { key: crypto.randomUUID(), text: '', icon: 'chat' }] }))
  const removeSuggestion = (key) => setForm((f) => ({ ...f, suggestions: f.suggestions.filter((s) => s.key !== key) }))

  async function save(e) {
    e.preventDefault()
    setError('')
    const next = {}
    if (!form.restaurantName.trim()) next.restaurantName = 'Informe o nome do restaurante.'
    if (!form.unitName.trim()) next.unitName = 'Informe o nome da unidade.'
    for (const s of form.suggestions) {
      if (!s.text.trim()) next[`suggestion-${s.key}`] = 'Escreva a pergunta ou remova a sugestão.'
    }
    setErrors(next)
    if (Object.keys(next).length) return

    setBusy(true)
    try {
      await api.put(`/api/units/${unit.id}/settings`, {
        restaurantName: form.restaurantName.trim(),
        cuisineType: form.cuisineType.trim() || null,
        unitName: form.unitName.trim(),
        city: form.city.trim() || null,
        comboBudgetTolerancePercent: Number(form.tolerance),
        chatSuggestions: form.suggestions.map((s) => ({ text: s.text.trim(), icon: s.icon })),
      })
      await reloadWorkspace()
      showToast('Configurações salvas.')
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  const budgetExample = 150
  const min = (budgetExample * (1 - form.tolerance / 100)).toFixed(0)
  const max = (budgetExample * (1 + form.tolerance / 100)).toFixed(0)

  return (
    <>
      <div className="page-head">
        <div>
          <h1>Configurações</h1>
          <p>Dados do restaurante e comportamento do garçom de IA.</p>
        </div>
      </div>
      <form className="settings-grid" onSubmit={save} noValidate>
        <Alert>{error}</Alert>
        <section className="card">
          <h2>Restaurante</h2>
          <div className="field-row">
            <Field label="Nome do restaurante" value={form.restaurantName} onChange={set('restaurantName')} error={errors.restaurantName} maxLength={160} />
            <Field label="Tipo de cozinha" value={form.cuisineType} onChange={set('cuisineType')} maxLength={60} />
          </div>
          <div className="field-row" style={{ marginTop: 16 }}>
            <Field label="Nome da unidade" value={form.unitName} onChange={set('unitName')} error={errors.unitName} maxLength={120} />
            <Field label="Cidade" value={form.city} onChange={set('city')} maxLength={120} />
          </div>
        </section>

        <section className="card">
          <h2>Combos por orçamento</h2>
          <div className="form-field">
            <label htmlFor="tolerance">Tolerância em relação ao orçamento do cliente</label>
            <div className="range-row">
              <input
                id="tolerance"
                type="range"
                min={0}
                max={50}
                step={5}
                value={form.tolerance}
                onChange={set('tolerance')}
                aria-describedby="tolerance-hint"
              />
              <output htmlFor="tolerance">±{form.tolerance}%</output>
            </div>
            <span id="tolerance-hint" className="hint">
              Para um combo de R$ {budgetExample}, o garçom monta opções entre R$ {min} e R$ {max}. Acima do valor pedido, ele avisa o cliente.
            </span>
          </div>
        </section>

        <section className="card" aria-labelledby="suggestions-title">
          <h2 id="suggestions-title">Sugestões do chat</h2>
          <p className="card-hint">
            Perguntas prontas na tela inicial do chat. Ao tocar, o cliente envia a pergunta ao garçom. O atalho “Ver
            cardápio completo” aparece sempre.
          </p>
          {form.suggestions.length > 0 && (
            <ol className="suggestion-editor">
              {form.suggestions.map((s, index) => (
                <li key={s.key} className="suggestion-row">
                  <div className="suggestion-row-main">
                    <Field
                      label={`Sugestão ${index + 1}`}
                      value={s.text}
                      onChange={(e) => setSuggestion(s.key, { text: e.target.value })}
                      error={errors[`suggestion-${s.key}`]}
                      maxLength={SUGGESTION_MAX_LENGTH}
                      placeholder="Ex.: Qual o prato do dia?"
                    />
                    <button
                      type="button"
                      className="icon-btn icon-btn--plain"
                      onClick={() => removeSuggestion(s.key)}
                      aria-label={`Remover sugestão ${index + 1}`}
                    >
                      <Icon name="trash" size={18} />
                    </button>
                  </div>
                  <fieldset className="icon-picker">
                    <legend>Ícone</legend>
                    {SUGGESTION_ICONS.map((icon) => (
                      <label key={icon.name} className="icon-choice" title={icon.label}>
                        <input
                          type="radio"
                          name={`icon-${s.key}`}
                          value={icon.name}
                          checked={s.icon === icon.name}
                          onChange={() => setSuggestion(s.key, { icon: icon.name })}
                        />
                        <span className="icon-choice-face">
                          <Icon name={icon.name} size={18} />
                        </span>
                        <span className="sr-only">{icon.label}</span>
                      </label>
                    ))}
                  </fieldset>
                </li>
              ))}
            </ol>
          )}
          {form.suggestions.length < MAX_SUGGESTIONS ? (
            <button type="button" className="btn btn--outline" onClick={addSuggestion}>
              <Icon name="plus" size={18} />
              Adicionar sugestão
            </button>
          ) : (
            <p className="hint">Limite de {MAX_SUGGESTIONS} sugestões atingido.</p>
          )}
        </section>

        <section className="card">
          <h2>Conta</h2>
          <p style={{ color: 'var(--ink-2)' }}>
            {workspace.ownerName} · {user?.email}
          </p>
        </section>

        <div>
          <button type="submit" className="btn btn--primary" disabled={busy}>
            {busy ? <Spinner label="Salvando" /> : 'Salvar alterações'}
          </button>
        </div>
      </form>
      {toast}
    </>
  )
}
