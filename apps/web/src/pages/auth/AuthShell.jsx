import { Link } from 'react-router-dom'
import DishShowcase from '../../components/DishShowcase'

/** Centered split card of the auth screens: brand pitch with an animated dish on the left, form on the right. */
export default function AuthShell({ children }) {
  return (
    <div className="auth">
      <div className="auth-box">
        <aside className="auth-aside">
          <Link to="/" className="auth-brand" aria-label="Meu Garçom — início">
            <img src="/logo-icon.png" alt="" width="44" height="18" />
            <span>Meu Garçom</span>
          </Link>
          <div className="auth-copy">
            <p className="auth-headline">Tudo que acontece na mesa, num QR code ou num toque.</p>
            <p>Um garçom de IA que responde sobre pratos, ingredientes e alergias, direto do seu cardápio real.</p>
          </div>
          <DishShowcase className="auth-showcase" />
        </aside>
        <main className="auth-main">
          <div className="auth-card">{children}</div>
        </main>
      </div>
    </div>
  )
}
