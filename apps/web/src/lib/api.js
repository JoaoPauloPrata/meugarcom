import { supabase } from './supabase'

const BASE_URL = (import.meta.env.VITE_API_URL || 'http://localhost:5080').replace(/\/$/, '')

const NETWORK_ERROR = 'Não foi possível falar com o servidor. Verifique sua conexão e tente novamente.'
const UNEXPECTED_ERROR = 'Algo deu errado. Tente novamente em instantes.'

/** Error returned by the API (ProblemDetails). `message` is already user-facing PT-BR text. */
export class ApiError extends Error {
  constructor(message, { status = 0, code = null, fields = {} } = {}) {
    super(message)
    this.status = status
    this.code = code
    this.fields = fields
  }
}

async function authHeader() {
  if (!supabase) return {}
  const { data } = await supabase.auth.getSession()
  const token = data.session?.access_token
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function request(method, path, { body, form, auth = true } = {}) {
  const headers = auth ? await authHeader() : {}
  let payload
  if (form) {
    payload = form
  } else if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
    payload = JSON.stringify(body)
  }

  let response
  try {
    response = await fetch(`${BASE_URL}${path}`, { method, headers, body: payload })
  } catch {
    throw new ApiError(NETWORK_ERROR)
  }

  if (response.status === 204) return null
  const data = await response.json().catch(() => null)
  if (response.ok) return data

  if (response.status === 401) {
    throw new ApiError('Sua sessão expirou. Entre novamente.', { status: 401, code: 'unauthorized' })
  }
  throw new ApiError(data?.detail || UNEXPECTED_ERROR, {
    status: response.status,
    code: data?.code ?? null,
    fields: data?.errors ?? {},
  })
}

export const api = {
  get: (path, opts) => request('GET', path, opts),
  post: (path, body, opts) => request('POST', path, { ...opts, body }),
  put: (path, body, opts) => request('PUT', path, { ...opts, body }),
  patch: (path, body, opts) => request('PATCH', path, { ...opts, body }),
  del: (path, opts) => request('DELETE', path, opts),
  upload: (path, form, method = 'POST') => request(method, path, { form }),
}

/** Public chat (no login). */
export const publicApi = {
  startConversation: (accessCode) => request('POST', '/api/public/conversations', { body: { accessCode }, auth: false }),
  sendMessage: (conversationId, content) =>
    request('POST', `/api/public/conversations/${conversationId}/messages`, { body: { content }, auth: false }),
  getMenu: (accessCode) => request('GET', `/api/public/menus/${encodeURIComponent(accessCode)}`, { auth: false }),
}
