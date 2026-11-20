import { useState } from 'react'
import { Link } from 'react-router-dom'
import { supabase } from '../../lib/supabase'
import { authErrorMessage } from '../../lib/auth'
import { Alert, Field, Spinner } from '../../components/ui'
import AuthShell from './AuthShell'

export default function ForgotPassword() {
  const [email, setEmail] = useState('')
  const [error, setError] = useState('')
  const [sent, setSent] = useState(false)
  const [submitting, setSubmitting] = useState(false)

  async function onSubmit(e) {
    e.preventDefault()
    setError('')
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      setError('Informe um e-mail válido.')
      return
    }
    setSubmitting(true)
    const { error: authError } = await supabase.auth.resetPasswordForEmail(email.trim(), {
      redirectTo: `${window.location.origin}/reset-password`,
    })
    setSubmitting(false)
    if (authError && authError.code?.startsWith('over_')) {
      setError(authErrorMessage(authError))
      return
    }
    // Same message whether or not the e-mail exists (no account enumeration).
    setSent(true)
  }

  return (
    <AuthShell>
      <h1>Recuperar senha</h1>
      <p className="auth-sub">Enviamos um link para você criar uma nova senha.</p>
      <Alert>{error}</Alert>
      {sent ? (
        <Alert kind="success">
          Se houver uma conta com <strong>{email.trim()}</strong>, você vai receber o link em instantes. Confira também a caixa de spam.
        </Alert>
      ) : (
        <form onSubmit={onSubmit} noValidate>
          <Field
            label="E-mail"
            type="email"
            autoComplete="email"
            inputMode="email"
            pill
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          <button type="submit" className="btn btn--primary btn--lg btn--block" disabled={submitting}>
            {submitting ? <Spinner label="Enviando" /> : 'Enviar link'}
          </button>
        </form>
      )}
      <p className="auth-switch">
        <Link to="/login">Voltar para o login</Link>
      </p>
    </AuthShell>
  )
}
