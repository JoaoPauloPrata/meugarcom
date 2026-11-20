import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { supabase } from '../../lib/supabase'
import { authErrorMessage, useAuth } from '../../lib/auth'
import { Alert, PasswordField, Spinner } from '../../components/ui'
import AuthShell from './AuthShell'

export default function ResetPassword() {
  const { session, loading } = useAuth()
  const navigate = useNavigate()
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function onSubmit(e) {
    e.preventDefault()
    setError('')
    if (password.length < 8) {
      setError('A senha precisa ter pelo menos 8 caracteres.')
      return
    }
    if (password !== confirm) {
      setError('As senhas não coincidem.')
      return
    }
    setSubmitting(true)
    const { error: authError } = await supabase.auth.updateUser({ password })
    setSubmitting(false)
    if (authError) {
      setError(authErrorMessage(authError))
      return
    }
    navigate('/dashboard', { replace: true })
  }

  return (
    <AuthShell>
      <h1>Nova senha</h1>
      {loading ? (
        <Spinner label="Carregando" />
      ) : !session ? (
        <>
          <Alert>Este link é inválido ou expirou. Peça um novo link de recuperação.</Alert>
          <Link to="/forgot-password" className="btn btn--primary btn--lg btn--block">
            Pedir novo link
          </Link>
        </>
      ) : (
        <>
          <p className="auth-sub">Escolha uma senha com pelo menos 8 caracteres.</p>
          <Alert>{error}</Alert>
          <form onSubmit={onSubmit} noValidate>
            <PasswordField
              label="Nova senha"
              autoComplete="new-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <div style={{ marginTop: 16 }}>
              <PasswordField
                label="Confirme a nova senha"
                autoComplete="new-password"
                value={confirm}
                onChange={(e) => setConfirm(e.target.value)}
              />
            </div>
            <button type="submit" className="btn btn--primary btn--lg btn--block" disabled={submitting}>
              {submitting ? <Spinner label="Salvando" /> : 'Salvar nova senha'}
            </button>
          </form>
        </>
      )}
    </AuthShell>
  )
}
