import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { supabase } from '../../lib/supabase'
import { api } from '../../lib/api'
import { authErrorMessage, signInWithGoogle, useAuth } from '../../lib/auth'
import Icon from '../../components/Icon'
import { Alert, BrandLink, Field, GoogleIcon, NumberStepper, PageLoader, PasswordField, Spinner } from '../../components/ui'

const STEPS = [
  { key: 'account', label: 'Sua conta' },
  { key: 'restaurant', label: 'Seu restaurante' },
  { key: 'plan', label: 'Plano' },
]

const CUISINES = [
  'Brasileira',
  'Italiana',
  'Pizzaria',
  'Japonesa',
  'Hamburgueria',
  'Bar e petiscos',
  'Frutos do mar',
  'Árabe',
  'Vegetariana',
  'Cafeteria',
  'Doceria',
  'Outra',
]

const PLANS = [
  {
    key: 'assistant',
    title: 'Assistente',
    price: '1º mês grátis',
    note: 'Depois, R$ 149,90/mês por unidade',
    available: true,
    features: [
      'Garçom de IA que conhece todo o seu cardápio',
      'Responde só com o que está no cardápio',
      'Importação do cardápio por PDF, foto ou planilha',
      'QR code para cada mesa',
      'O pedido continua com o seu garçom',
    ],
  },
  {
    key: 'orders',
    title: 'Assistente + Pedidos',
    price: 'Em breve',
    note: 'Por unidade',
    available: false,
    features: ['Tudo do Assistente', 'Cliente pede pelo chat', 'Pedidos chegam na cozinha'],
  },
  {
    key: 'payments',
    title: 'Pedidos + Pagamento',
    price: 'Em breve',
    note: 'Por unidade',
    available: false,
    features: ['Tudo de Pedidos', 'Pix gerado no chat', 'Pagamentos avisados no painel'],
  },
]

export default function Signup() {
  const { session, user, loading } = useAuth()
  const navigate = useNavigate()
  const [step, setStep] = useState('account')
  const [checkingWorkspace, setCheckingWorkspace] = useState(false)

  const [account, setAccount] = useState({ name: '', email: '', password: '' })
  const [restaurant, setRestaurant] = useState({ name: '', city: '', cuisineType: '', tableCount: 10 })
  const [errors, setErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  // Signed-in users skip the account step; users who already finished go to the dashboard.
  useEffect(() => {
    if (loading || !session || step !== 'account') return
    let active = true
    setCheckingWorkspace(true)
    api
      .get('/api/me/workspace')
      .then(() => active && navigate('/dashboard', { replace: true }))
      .catch(() => active && setStep('restaurant'))
      .finally(() => active && setCheckingWorkspace(false))
    return () => {
      active = false
    }
  }, [loading, session, step, navigate])

  const currentIndex = STEPS.findIndex((s) => s.key === step)

  async function submitAccount(e) {
    e.preventDefault()
    setFormError('')
    const next = {}
    if (!account.name.trim()) next.name = 'Informe seu nome.'
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(account.email.trim())) next.email = 'Informe um e-mail válido.'
    if (account.password.length < 8) next.password = 'Use pelo menos 8 caracteres.'
    setErrors(next)
    if (Object.keys(next).length) return

    setSubmitting(true)
    const { data, error } = await supabase.auth.signUp({
      email: account.email.trim(),
      password: account.password,
      options: {
        data: { full_name: account.name.trim() },
        emailRedirectTo: `${window.location.origin}/signup`,
      },
    })
    setSubmitting(false)
    if (error) {
      setFormError(authErrorMessage(error))
      return
    }
    // With e-mail confirmation on, Supabase returns no session until the link is clicked.
    setStep(data.session ? 'restaurant' : 'confirm-email')
  }

  function submitRestaurant(e) {
    e.preventDefault()
    const next = {}
    if (!restaurant.name.trim()) next.restaurantName = 'Informe o nome do restaurante.'
    setErrors(next)
    if (Object.keys(next).length) return
    setStep('plan')
  }

  async function finish() {
    setFormError('')
    setSubmitting(true)
    const ownerName = user?.user_metadata?.full_name || account.name.trim() || user?.email?.split('@')[0] || 'Responsável'
    try {
      await api.post('/api/onboarding', {
        ownerName,
        restaurantName: restaurant.name.trim(),
        city: restaurant.city.trim() || null,
        cuisineType: restaurant.cuisineType || null,
        tableCount: restaurant.tableCount,
      })
      navigate('/dashboard', { replace: true })
    } catch (error) {
      if (error.code === 'account.already_exists') {
        navigate('/dashboard', { replace: true })
        return
      }
      setFormError(error.message)
      setSubmitting(false)
    }
  }

  async function onGoogle() {
    setFormError('')
    const { error } = await signInWithGoogle('/signup')
    if (error) setFormError(error)
  }

  if (loading || checkingWorkspace) {
    return (
      <div className="page-loading">
        <PageLoader />
      </div>
    )
  }

  return (
    <div className="signup">
      <div className="signup-top">
        <BrandLink />
        {step === 'account' && (
          <span className="auth-switch" style={{ margin: 0 }}>
            Já tem conta? <Link to="/login">Entrar</Link>
          </span>
        )}
      </div>

      {step !== 'confirm-email' && (
        <ol className="signup-steps" aria-label="Etapas do cadastro">
          {STEPS.map((s, i) => (
            <li
              key={s.key}
              className={i < currentIndex ? 'is-done' : i === currentIndex ? 'is-current' : ''}
              aria-current={i === currentIndex ? 'step' : undefined}
            >
              <span className="dot" aria-hidden="true" />
              {s.label}
            </li>
          ))}
        </ol>
      )}

      <section className="signup-panel" style={step === 'confirm-email' ? { marginTop: 48 } : undefined}>
        {step === 'account' && (
          <div className="signup-grid">
            <form className="signup-form" onSubmit={submitAccount} noValidate>
              <h1>Crie sua conta</h1>
              <p className="signup-sub">Leva poucos minutos. Depois é só importar o cardápio.</p>
              <Alert>{formError}</Alert>
              <Field
                label="Seu nome"
                autoComplete="name"
                value={account.name}
                error={errors.name}
                onChange={(e) => setAccount({ ...account, name: e.target.value })}
              />
              <Field
                label="E-mail"
                type="email"
                inputMode="email"
                autoComplete="email"
                value={account.email}
                error={errors.email}
                onChange={(e) => setAccount({ ...account, email: e.target.value })}
              />
              <div style={{ marginTop: 16 }}>
                <PasswordField
                  autoComplete="new-password"
                  hint="Pelo menos 8 caracteres."
                  value={account.password}
                  error={errors.password}
                  onChange={(e) => setAccount({ ...account, password: e.target.value })}
                />
              </div>
              <div className="signup-actions">
                <button type="button" className="btn btn--ghost google-btn" onClick={onGoogle}>
                  <GoogleIcon />
                  Continuar com Google
                </button>
                <button type="submit" className="btn btn--primary" disabled={submitting}>
                  {submitting ? <Spinner label="Criando conta" /> : 'Continuar'}
                </button>
              </div>
            </form>
            <PhonePreview restaurantName="Seu restaurante" />
          </div>
        )}

        {step === 'confirm-email' && (
          <div className="confirm-email">
            <div className="confirm-icon">
              <Icon name="mail" size={28} />
            </div>
            <h1>Confirme seu e-mail</h1>
            <p>
              Enviamos um link para <strong>{account.email.trim()}</strong>. Clique nele para ativar a conta e continuar o
              cadastro do restaurante.
            </p>
            <Link to="/login" className="btn btn--outline">
              Já confirmei, quero entrar
            </Link>
          </div>
        )}

        {step === 'restaurant' && (
          <div className="signup-grid">
            <form className="signup-form" onSubmit={submitRestaurant} noValidate>
              <h1>Seu restaurante</h1>
              <p className="signup-sub">Essas informações aparecem para o cliente no chat.</p>
              <Field
                label="Nome do restaurante"
                autoComplete="organization"
                value={restaurant.name}
                error={errors.restaurantName}
                onChange={(e) => setRestaurant({ ...restaurant, name: e.target.value })}
              />
              <div className="field-row" style={{ marginTop: 16 }}>
                <Field
                  label="Cidade"
                  autoComplete="address-level2"
                  value={restaurant.city}
                  onChange={(e) => setRestaurant({ ...restaurant, city: e.target.value })}
                />
                <Field
                  as="select"
                  label="Tipo de cozinha"
                  value={restaurant.cuisineType}
                  onChange={(e) => setRestaurant({ ...restaurant, cuisineType: e.target.value })}
                >
                  <option value="">Selecione</option>
                  {CUISINES.map((c) => (
                    <option key={c} value={c}>
                      {c}
                    </option>
                  ))}
                </Field>
              </div>
              <div className="form-field" style={{ marginTop: 16 }}>
                <div className="tables-field">
                  <span className="field-label" id="tables-label">
                    Número de mesas
                  </span>
                  <NumberStepper
                    label="Número de mesas"
                    value={restaurant.tableCount}
                    min={0}
                    max={300}
                    onChange={(tableCount) => setRestaurant({ ...restaurant, tableCount })}
                  />
                </div>
                <span className="hint">Cada mesa ganha um QR code próprio. Dá para mudar depois.</span>
              </div>
              <div className="signup-actions">
                <span />
                <button type="submit" className="btn btn--primary">
                  Continuar
                </button>
              </div>
            </form>
            <PhonePreview restaurantName={restaurant.name.trim() || 'Seu restaurante'} />
          </div>
        )}

        {step === 'plan' && (
          <div>
            <div className="plans-head">
              <h1>Escolha seu plano</h1>
              <p>Comece pelo garçom de IA. Pedidos e pagamento chegam nas próximas versões.</p>
            </div>
            <Alert>{formError}</Alert>
            <div className="plan-grid">
              {PLANS.map((plan) => (
                <article
                  key={plan.key}
                  className={`plan-card ${plan.available ? 'plan-card--featured' : 'plan-card--soon'}`}
                >
                  {plan.available && <div className="plan-badge-top">Disponível agora</div>}
                  <h2>{plan.title}</h2>
                  <p className="plan-price-tag">{plan.price}</p>
                  <p className="plan-note">{plan.note}</p>
                  <ul>
                    {plan.features.map((f) => (
                      <li key={f}>
                        <Icon name="check" size={16} />
                        {f}
                      </li>
                    ))}
                  </ul>
                  {plan.available ? (
                    <button type="button" className="btn btn--primary btn--block" onClick={finish} disabled={submitting}>
                      {submitting ? <Spinner label="Criando restaurante" /> : 'Começar grátis'}
                    </button>
                  ) : (
                    <button type="button" className="btn btn--ghost btn--block" disabled>
                      Em breve
                    </button>
                  )}
                </article>
              ))}
            </div>
            <div className="signup-actions">
              <button type="button" className="btn btn--ghost" onClick={() => setStep('restaurant')} disabled={submitting}>
                Voltar
              </button>
            </div>
          </div>
        )}
      </section>
    </div>
  )
}

function PhonePreview({ restaurantName }) {
  return (
    <div className="preview-phone" aria-hidden="true">
      <div className="preview-phone-notch" />
      <div className="preview-phone-screen">
        <div className="preview-phone-head">
          <img src="/logo-icon.png" alt="" />
          <div>
            <strong>Meu Garçom</strong>
            <span>{restaurantName} · Mesa 1</span>
          </div>
        </div>
        <div className="preview-phone-bubble preview-phone-bubble--user">Tem algo leve, sem lactose?</div>
        <div className="preview-phone-bubble preview-phone-bubble--bot">
          Temos duas opções marcadas como sem lactose no cardápio. Quer que eu mostre?
        </div>
        <div className="preview-phone-bubble preview-phone-bubble--user">Sim!</div>
        <div className="preview-phone-input">Pergunte ao garçom…</div>
      </div>
    </div>
  )
}
