import { useState } from 'react'
import { joinWaitlist } from '../lib/supabase'

const EMPTY = { nome: '', email: '', whatsapp: '', restaurante: '', cidade: '', unidades: '1' }

function formatPhone(value) {
  const d = value.replace(/\D/g, '').slice(0, 11)
  if (d.length <= 2) return d
  if (d.length <= 7) return `(${d.slice(0, 2)}) ${d.slice(2)}`
  return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`
}

function validate(v) {
  const e = {}
  if (!v.nome.trim()) e.nome = 'Conta pra gente seu nome.'
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v.email.trim())) e.email = 'Confira o e-mail — algo não bateu.'
  if (v.whatsapp && v.whatsapp.replace(/\D/g, '').length < 10) e.whatsapp = 'Use DDD + número.'
  if (!v.restaurante.trim()) e.restaurante = 'Qual o nome do seu restaurante?'
  return e
}

export default function WaitlistForm() {
  const [values, setValues] = useState(EMPTY)
  const [errors, setErrors] = useState({})
  const [status, setStatus] = useState('idle') // idle | sending | done | error

  const set = (field) => (e) => {
    const value = field === 'whatsapp' ? formatPhone(e.target.value) : e.target.value
    setValues((v) => ({ ...v, [field]: value }))
    if (errors[field]) setErrors((er) => ({ ...er, [field]: undefined }))
  }

  async function onSubmit(e) {
    e.preventDefault()
    const found = validate(values)
    setErrors(found)
    if (Object.keys(found).length) {
      document.getElementById(`wl-${Object.keys(found)[0]}`)?.focus()
      return
    }
    setStatus('sending')
    const res = await joinWaitlist({
      nome: values.nome.trim(),
      email: values.email.trim().toLowerCase(),
      whatsapp: values.whatsapp || null,
      restaurante: values.restaurante.trim(),
      cidade: values.cidade.trim() || null,
      unidades: values.unidades,
      origem: 'landing',
    })
    setStatus(res.ok ? 'done' : 'error')
  }

  if (status === 'done') {
    return (
      <div className="form-done" role="status">
        <span className="form-done-icon" aria-hidden="true">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none">
            <path d="M5 12.5l4.5 4.5L19 7.5" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
        </span>
        <h3>Você está na lista, {values.nome.split(' ')[0]}.</h3>
        <p>Vamos chamar os restaurantes por ordem de inscrição. Fica de olho no seu e-mail{values.whatsapp ? ' e no WhatsApp' : ''}.</p>
      </div>
    )
  }

  const field = (name, label, props = {}, hint) => (
    <div className={`field ${errors[name] ? 'field--error' : ''}`}>
      <label htmlFor={`wl-${name}`}>{label}</label>
      <input
        id={`wl-${name}`}
        name={name}
        value={values[name]}
        onChange={set(name)}
        aria-invalid={!!errors[name]}
        aria-describedby={errors[name] ? `wl-${name}-err` : hint ? `wl-${name}-hint` : undefined}
        {...props}
      />
      {errors[name] ? (
        <span className="field-error" id={`wl-${name}-err`}>
          {errors[name]}
        </span>
      ) : (
        hint && (
          <span className="field-hint" id={`wl-${name}-hint`}>
            {hint}
          </span>
        )
      )}
    </div>
  )

  return (
    <form className="form" onSubmit={onSubmit} noValidate>
      <div className="form-grid">
        {field('nome', 'Seu nome', { autoComplete: 'name', placeholder: 'Ana Souza' })}
        {field('email', 'E-mail', { type: 'email', autoComplete: 'email', inputMode: 'email', placeholder: 'ana@restaurante.com.br' })}
        {field('whatsapp', 'WhatsApp', { type: 'tel', autoComplete: 'tel', inputMode: 'tel', placeholder: '(11) 91234-5678' }, 'Opcional')}
        {field('restaurante', 'Restaurante', { autoComplete: 'organization', placeholder: 'Bistrô Linho' })}
        {field('cidade', 'Cidade', { autoComplete: 'address-level2', placeholder: 'São Paulo, SP' }, 'Opcional')}
        <div className="field">
          <label htmlFor="wl-unidades">Unidades</label>
          <select id="wl-unidades" value={values.unidades} onChange={set('unidades')}>
            <option value="1">1 unidade</option>
            <option value="2-5">2 a 5 unidades</option>
            <option value="6+">6 ou mais</option>
          </select>
        </div>
      </div>
      <button className="btn btn--primary btn--lg" type="submit" disabled={status === 'sending'}>
        {status === 'sending' ? 'Enviando…' : 'Entrar na lista de espera'}
      </button>
      {status === 'error' && (
        <p className="form-error" role="alert">
          Não conseguimos salvar agora. Tenta de novo em instantes?
        </p>
      )}
      <p className="form-note">Sem spam. Só avisamos quando sua vaga abrir.</p>
    </form>
  )
}
