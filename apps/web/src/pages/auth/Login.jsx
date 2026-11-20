import { useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { supabase } from '../../lib/supabase'
import { authErrorMessage, signInWithGoogle, useAuth } from '../../lib/auth'
import { Alert, Field, GoogleIcon, PasswordField, Spinner } from '../../components/ui'
import AuthShell from './AuthShell'

export default function Login() {
  const { session, loading } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [googleBusy, setGoogleBusy] = useState(false)

  if (!loading && session) return <Navigate to="/dashboard" replace />

  async function onSubmit(e) {
    e.preventDefault()
    setError('')
    if (!email.trim() || !password) {
      setError('Informe seu e-mail e sua senha.')
      return
    }
    setSubmitting(true)
    const { error: authError } = await supabase.auth.signInWithPassword({ email: email.trim(), password })
    setSubmitting(false)
    if (authError) {
      setError(authErrorMessage(authError))
      return
    }
    navigate('/dashboard', { replace: true })
  }

  async function onGoogle() {
    setError('')
    setGoogleBusy(true)
    const { error: googleError } = await signInWithGoogle('/dashboard')
    setGoogleBusy(false)
    if (googleError) setError(googleError)
  }

  return (
    <AuthShell>
      <h1>Entrar</h1>
      <p className="auth-sub">Acesse o painel do seu restaurante.</p>
      <Alert>{error}</Alert>
      <form onSubmit={onSubmit} noValidate>
        <Field
          label="E-mail"
          type="email"
          name="email"
          autoComplete="email"
          inputMode="email"
          pill
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <div style={{ marginTop: 16 }}>
          <PasswordField
            name="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <Link to="/forgot-password" className="auth-forgot">
          Esqueci minha senha
        </Link>
        <button type="submit" className="btn btn--primary btn--lg btn--block" disabled={submitting}>
          {submitting ? <Spinner label="Entrando" /> : 'Entrar'}
        </button>
      </form>
      <div className="divider">ou</div>
      <button type="button" className="btn btn--ghost btn--lg btn--block google-btn" onClick={onGoogle} disabled={googleBusy}>
        {googleBusy ? <Spinner label="Abrindo o Google" /> : <GoogleIcon />}
        Continuar com Google
      </button>
      <p className="auth-switch">
        Ainda não tem conta? <Link to="/signup">Criar conta</Link>
      </p>
    </AuthShell>
  )
}
