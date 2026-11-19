import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { supabase } from './supabase'

const AuthContext = createContext(null)

const AUTH_ERRORS = {
  invalid_credentials: 'E-mail ou senha incorretos.',
  email_not_confirmed: 'Confirme seu e-mail antes de entrar. Procure a mensagem que enviamos.',
  user_already_exists: 'Já existe uma conta com este e-mail. Tente entrar.',
  email_exists: 'Já existe uma conta com este e-mail. Tente entrar.',
  weak_password: 'Senha fraca. Use pelo menos 8 caracteres, com letras e números.',
  over_email_send_rate_limit: 'Muitas tentativas. Aguarde alguns minutos e tente de novo.',
  over_request_rate_limit: 'Muitas tentativas. Aguarde alguns minutos e tente de novo.',
  email_address_invalid: 'Este e-mail não é válido.',
  same_password: 'A nova senha precisa ser diferente da anterior.',
}

/** Translates a Supabase Auth error into a PT-BR message for the screen. */
export function authErrorMessage(error) {
  if (!error) return ''
  return AUTH_ERRORS[error.code] || 'Não foi possível concluir. Tente novamente.'
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!supabase) {
      setLoading(false)
      return
    }
    supabase.auth.getSession().then(({ data }) => {
      setSession(data.session)
      setLoading(false)
    })
    const { data } = supabase.auth.onAuthStateChange((_event, next) => setSession(next))
    return () => data.subscription.unsubscribe()
  }, [])

  const value = useMemo(
    () => ({
      session,
      user: session?.user ?? null,
      loading,
      signOut: () => supabase?.auth.signOut(),
    }),
    [session, loading],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}

/**
 * Google sign-in through Supabase OAuth. When the provider is not enabled yet on the project,
 * returns a friendly message instead of sending the user to an error page.
 */
export async function signInWithGoogle(redirectPath = '/dashboard') {
  if (!supabase) return { error: 'Login indisponível no momento.' }
  try {
    const url = import.meta.env.VITE_SUPABASE_URL
    const key = import.meta.env.VITE_SUPABASE_ANON_KEY
    const response = await fetch(`${url}/auth/v1/settings`, { headers: { apikey: key } })
    const settings = await response.json()
    if (!settings?.external?.google) {
      return { error: 'O login com Google ainda não está disponível. Use e-mail e senha por enquanto.' }
    }
  } catch {
    return { error: 'Não foi possível iniciar o login com Google. Tente novamente.' }
  }
  const { error } = await supabase.auth.signInWithOAuth({
    provider: 'google',
    options: { redirectTo: `${window.location.origin}${redirectPath}` },
  })
  return { error: error ? 'Não foi possível iniciar o login com Google. Tente novamente.' : null }
}
