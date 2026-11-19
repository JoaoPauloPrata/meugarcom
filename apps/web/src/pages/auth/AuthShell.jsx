import { Link } from 'react-router-dom'
import Icon from '../../components/Icon'

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
          <div className="auth-showcase" aria-hidden="true">
            <div className="auth-pizza">
              <img src="/pratos/pizza-margherita.webp" alt="" width="300" height="300" />
            </div>
            <span className="auth-chip auth-chip--photo">
              <Icon name="chat" size={16} strokeWidth={2.2} />
              Foto do prato
            </span>
            <span className="auth-chip auth-chip--price">
              <Icon name="check" size={16} strokeWidth={2.6} />
              R$ 59
            </span>
            <span className="auth-chip auth-chip--ingredients">
              <Icon name="heart" size={16} strokeWidth={2.2} />
              Tomate, búfala, manjericão
            </span>
          </div>
        </aside>
        <main className="auth-main">
          <div className="auth-card">{children}</div>
        </main>
      </div>
    </div>
  )
}
