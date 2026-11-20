import { useCallback, useEffect, useId, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import Icon from './Icon'

export function BrandLink({ className = 'logo', to = '/' }) {
  return (
    <Link to={to} className={className} aria-label="Meu Garçom — início">
      <img src="/logo-icon.png" alt="" width="50" height="20" />
      <span>Meu Garçom</span>
    </Link>
  )
}

export function Spinner({ label }) {
  return (
    <>
      <span className="spinner" aria-hidden="true" />
      {label && <span className="sr-only">{label}</span>}
    </>
  )
}

export function Alert({ kind = 'error', children }) {
  if (!children) return null
  const icon = kind === 'success' ? 'check' : kind === 'info' ? 'sparkles' : 'alert'
  return (
    <div className={`form-alert form-alert--${kind}`} role={kind === 'error' ? 'alert' : 'status'}>
      <Icon name={icon} size={18} />
      <div>{children}</div>
    </div>
  )
}

/** Text input with a visible label, hint and inline error wired with aria-describedby. */
export function Field({ label, hint, error, as = 'input', className = '', children, ...props }) {
  const id = useId()
  const hintId = hint ? `${id}-hint` : undefined
  const errorId = error ? `${id}-error` : undefined
  const describedBy = [hintId, errorId].filter(Boolean).join(' ') || undefined
  const Control = as
  const controlClass = as === 'textarea' ? 'textarea' : as === 'select' ? 'select' : 'input'
  return (
    <div className={`form-field ${className}`}>
      <label htmlFor={id}>{label}</label>
      <Control
        id={id}
        className={`${controlClass} ${props.pill ? 'input--pill' : ''}`}
        aria-invalid={error ? 'true' : undefined}
        aria-describedby={describedBy}
        {...props}
        pill={undefined}
      >
        {children}
      </Control>
      {hint && (
        <span id={hintId} className="hint">
          {hint}
        </span>
      )}
      {error && (
        <span id={errorId} className="field-error">
          {error}
        </span>
      )}
    </div>
  )
}

export function PasswordField({ label = 'Senha', error, hint, ...props }) {
  const id = useId()
  const [visible, setVisible] = useState(false)
  const errorId = error ? `${id}-error` : undefined
  const hintId = hint ? `${id}-hint` : undefined
  return (
    <div className="form-field">
      <label htmlFor={id}>{label}</label>
      <div className="password-wrap">
        <input
          id={id}
          type={visible ? 'text' : 'password'}
          className="input input--pill"
          aria-invalid={error ? 'true' : undefined}
          aria-describedby={[hintId, errorId].filter(Boolean).join(' ') || undefined}
          {...props}
        />
        <button
          type="button"
          className="password-toggle"
          onClick={() => setVisible((v) => !v)}
          aria-pressed={visible}
          aria-label={visible ? 'Ocultar senha' : 'Mostrar senha'}
        >
          {visible ? 'Ocultar' : 'Mostrar'}
        </button>
      </div>
      {hint && (
        <span id={hintId} className="hint">
          {hint}
        </span>
      )}
      {error && (
        <span id={errorId} className="field-error">
          {error}
        </span>
      )}
    </div>
  )
}

export function Switch({ checked, onChange, label, disabled }) {
  return (
    <label className="switch">
      <input
        type="checkbox"
        role="switch"
        checked={checked}
        disabled={disabled}
        onChange={(e) => onChange(e.target.checked)}
        aria-label={label}
      />
      <span className="switch-track" aria-hidden="true" />
    </label>
  )
}

export function NumberStepper({ value, onChange, min = 0, max = 300, label }) {
  const clamp = (n) => Math.min(max, Math.max(min, Number.isFinite(n) ? n : min))
  return (
    <div className="stepper" role="group" aria-label={label}>
      <button type="button" onClick={() => onChange(clamp(value - 1))} disabled={value <= min} aria-label="Diminuir">
        <Icon name="minus" size={16} />
      </button>
      <input
        type="number"
        inputMode="numeric"
        value={value}
        min={min}
        max={max}
        aria-label={label}
        onChange={(e) => onChange(clamp(parseInt(e.target.value, 10)))}
      />
      <button type="button" onClick={() => onChange(clamp(value + 1))} disabled={value >= max} aria-label="Aumentar">
        <Icon name="plus" size={16} />
      </button>
    </div>
  )
}

const FOCUSABLE = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'

/** Dialog with focus trap, Esc to close and focus restore. variant: 'drawer' | 'modal' | 'modal-sm' | 'sheet'. */
export function Dialog({ title, onClose, children, footer, variant = 'modal' }) {
  const panelRef = useRef(null)
  const onCloseRef = useRef(onClose)
  const titleId = useId()
  onCloseRef.current = onClose

  useEffect(() => {
    const previous = document.activeElement
    const panel = panelRef.current
    const first = panel?.querySelector('[data-autofocus]') || panel?.querySelector(FOCUSABLE)
    first?.focus()
    const onKey = (e) => {
      if (e.key === 'Escape') {
        e.stopPropagation()
        onCloseRef.current()
      }
      if (e.key === 'Tab' && panel) {
        const nodes = [...panel.querySelectorAll(FOCUSABLE)]
        if (nodes.length === 0) return
        const firstNode = nodes[0]
        const lastNode = nodes[nodes.length - 1]
        if (e.shiftKey && document.activeElement === firstNode) {
          e.preventDefault()
          lastNode.focus()
        } else if (!e.shiftKey && document.activeElement === lastNode) {
          e.preventDefault()
          firstNode.focus()
        }
      }
    }
    document.addEventListener('keydown', onKey)
    const overflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      document.removeEventListener('keydown', onKey)
      document.body.style.overflow = overflow
      previous?.focus?.()
    }
  }, [])

  const overlayClass = variant === 'drawer' ? 'overlay' : variant === 'sheet' ? 'sheet-overlay' : 'overlay overlay--center'
  const panelClass = variant === 'drawer' ? 'drawer' : variant === 'sheet' ? 'sheet' : variant === 'modal-sm' ? 'modal modal--sm' : 'modal'

  return (
    <div className={overlayClass} onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <div ref={panelRef} className={panelClass} role="dialog" aria-modal="true" aria-labelledby={titleId}>
        {variant === 'sheet' && <div className="sheet-handle" aria-hidden="true" />}
        <div className="panel-head">
          <h2 id={titleId}>{title}</h2>
          <button type="button" className="icon-btn icon-btn--plain" onClick={onClose} aria-label="Fechar">
            <Icon name="close" />
          </button>
        </div>
        <div className="panel-body">{children}</div>
        {footer && <div className="panel-foot">{footer}</div>}
      </div>
    </div>
  )
}

export function useToast() {
  const [message, setMessage] = useState('')
  const timer = useRef(null)
  const show = useCallback((text) => {
    clearTimeout(timer.current)
    setMessage(text)
    timer.current = setTimeout(() => setMessage(''), 2800)
  }, [])
  useEffect(() => () => clearTimeout(timer.current), [])
  const node = message ? (
    <div className="toast" role="status" aria-live="polite">
      {message}
    </div>
  ) : null
  return [node, show]
}

export function GoogleIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 48 48" aria-hidden="true">
      <path fill="#FFC107" d="M43.6 20.5H42V20H24v8h11.3C33.7 32.7 29.2 36 24 36c-6.6 0-12-5.4-12-12s5.4-12 12-12c3.1 0 5.8 1.2 7.9 3.1l5.7-5.7C34 6.1 29.3 4 24 4 12.9 4 4 12.9 4 24s8.9 20 20 20 20-8.9 20-20c0-1.3-.1-2.4-.4-3.5z" />
      <path fill="#FF3D00" d="m6.3 14.7 6.6 4.8C14.7 15.1 19 12 24 12c3.1 0 5.8 1.2 7.9 3.1l5.7-5.7C34 6.1 29.3 4 24 4 16.3 4 9.7 8.3 6.3 14.7z" />
      <path fill="#4CAF50" d="M24 44c5.2 0 9.9-2 13.4-5.2l-6.2-5.2C29.2 35.1 26.7 36 24 36c-5.2 0-9.6-3.3-11.3-7.9l-6.5 5C9.5 39.6 16.2 44 24 44z" />
      <path fill="#1976D2" d="M43.6 20.5H42V20H24v8h11.3c-.8 2.2-2.2 4.2-4.1 5.6l6.2 5.2C37 39.2 44 34 44 24c0-1.3-.1-2.4-.4-3.5z" />
    </svg>
  )
}
