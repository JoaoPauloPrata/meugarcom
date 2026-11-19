import { useState } from 'react'
import { useOutletContext } from 'react-router-dom'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Alert, Field, Spinner, useToast } from '../../components/ui'

export default function SettingsPage() {
  const { workspace, unit, reloadWorkspace } = useOutletContext()
  const { user } = useAuth()
  const [form, setForm] = useState({
    restaurantName: workspace.restaurantName,
    cuisineType: workspace.cuisineType ?? '',
    unitName: unit.name,
    city: unit.city ?? '',
    tolerance: Math.round(unit.comboBudgetTolerance * 100),
  })
  const [errors, setErrors] = useState({})
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [toast, showToast] = useToast()

  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }))

  async function save(e) {
    e.preventDefault()
    setError('')
    const next = {}
    if (!form.restaurantName.trim()) next.restaurantName = 'Informe o nome do restaurante.'
    if (!form.unitName.trim()) next.unitName = 'Informe o nome da unidade.'
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
