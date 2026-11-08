import { createClient } from '@supabase/supabase-js'

const url = import.meta.env.VITE_SUPABASE_URL
const key = import.meta.env.VITE_SUPABASE_ANON_KEY

export const supabase = url && key ? createClient(url, key) : null

export async function joinWaitlist(entry) {
  if (!supabase) {
    // Supabase ainda não configurado: simula sucesso para testar o fluxo.
    console.warn('[waitlist] Supabase não configurado — dados não foram salvos:', entry)
    await new Promise((r) => setTimeout(r, 700))
    return { ok: true }
  }
  const { error } = await supabase.from('waitlist').insert(entry)
  if (!error) return { ok: true }
  if (error.code === '23505') return { ok: true, duplicate: true }
  return { ok: false, error }
}
