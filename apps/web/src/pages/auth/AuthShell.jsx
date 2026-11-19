import { Link } from 'react-router-dom'

/** Split layout of the auth screens (reference: Screen 1 — Login). */
export default function AuthShell({ children }) {
  return (
    <div className="auth">
      <aside className="auth-aside">
        <Link to="/" className="auth-brand" aria-label="Meu Garçom — início">
          <img src="/logo-icon.png" alt="" width="72" height="29" />
          <span>Meu Garçom</span>
        </Link>
        <p className="auth-headline">Tudo que acontece na mesa, num QR code ou num toque.</p>
        <img className="auth-photo" src="/pratos/peixe-grelhado.jpg" alt="" loading="lazy" />
      </aside>
      <main className="auth-main">
        <div className="auth-card">{children}</div>
      </main>
    </div>
  )
}
