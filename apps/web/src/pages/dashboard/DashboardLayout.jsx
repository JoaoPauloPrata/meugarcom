import { useCallback, useEffect, useState } from 'react'
import { Navigate, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import Icon from '../../components/Icon'
import { Alert, PageLoader } from '../../components/ui'

const NAV = [
  { to: '/dashboard', label: 'Visão geral', icon: 'home', end: true },
  { to: '/dashboard/menu', label: 'Cardápio', icon: 'menu' },
  { to: '/dashboard/tables', label: 'Mesas e QR', icon: 'qr' },
  { to: '/dashboard/conversations', label: 'Conversas', icon: 'chat' },
  { to: '/dashboard/settings', label: 'Configurações', icon: 'settings' },
]

export default function DashboardLayout() {
  const { session, user, loading, signOut } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [workspace, setWorkspace] = useState(null)
  const [error, setError] = useState('')
  const [menuOpen, setMenuOpen] = useState(false)

  const loadWorkspace = useCallback(async () => {
    try {
      setWorkspace(await api.get('/api/me/workspace'))
      setError('')
    } catch (e) {
      if (e.code === 'account.not_found') navigate('/signup', { replace: true })
      else if (e.status === 401) {
        await signOut()
        navigate('/login', { replace: true })
      } else setError(e.message)
    }
  }, [navigate, signOut])

  useEffect(() => {
    if (session) loadWorkspace()
  }, [session, loadWorkspace])

  useEffect(() => setMenuOpen(false), [location.pathname])

  if (!loading && !session) return <Navigate to="/login" replace />

  if (!workspace) {
    return (
      <div className="page-loading">
        {error ? (
          <div style={{ maxWidth: 420, padding: 16 }}>
            <Alert>{error}</Alert>
            <button type="button" className="btn btn--primary" onClick={loadWorkspace}>
              Tentar de novo
            </button>
          </div>
        ) : (
          <PageLoader label="Carregando o painel" />
        )}
      </div>
    )
  }

  const unit = workspace.units[0]
  const initials = (workspace.ownerName || user?.email || '?')
    .split(' ')
    .map((p) => p[0])
    .slice(0, 2)
    .join('')
    .toUpperCase()

  async function onSignOut() {
    await signOut()
    navigate('/login', { replace: true })
  }

  return (
    <div className="dashboard">
      <a href="#dash-content" className="skip-link">
        Pular para o conteúdo
      </a>
      {menuOpen && <div className="dash-scrim" onClick={() => setMenuOpen(false)} aria-hidden="true" />}
      <aside className={`dash-sidebar ${menuOpen ? 'is-open' : ''}`} aria-label="Menu do painel">
        <NavLink to="/dashboard" className="dash-brand" aria-label="Meu Garçom — visão geral">
          <img src="/logo-icon.png" alt="" width="44" height="18" />
          <span>Meu Garçom</span>
        </NavLink>
        <nav className="dash-nav">
          {NAV.map((item) => (
            <NavLink key={item.to} to={item.to} end={item.end}>
              <Icon name={item.icon} size={20} />
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="dash-sidebar-foot dash-nav">
          <a href={`/m/${unit.publicCode}`} target="_blank" rel="noreferrer">
            <Icon name="external" size={20} />
            Testar o chat
          </a>
          <button type="button" className="dash-nav-btn" onClick={onSignOut}>
            <Icon name="logout" size={20} />
            Sair
          </button>
        </div>
      </aside>

      <div className="dash-main">
        <header className="dash-topbar">
          <button
            type="button"
            className="icon-btn dash-menu-btn"
            onClick={() => setMenuOpen(true)}
            aria-label="Abrir menu"
            aria-expanded={menuOpen}
          >
            <Icon name="hamburger" />
          </button>
          <div className="restaurant-name">
            <strong>{workspace.restaurantName}</strong>
            <span>{unit.name}</span>
          </div>
          <div className="topbar-actions">
            <span className="avatar" aria-hidden="true" title={user?.email}>
              {initials}
            </span>
            <div className="topbar-user">
              <strong>{workspace.ownerName || 'Minha conta'}</strong>
              <span>{user?.email}</span>
            </div>
          </div>
        </header>
        <main id="dash-content" className="dash-content">
          <Outlet context={{ workspace, unit, reloadWorkspace: loadWorkspace }} />
        </main>
      </div>
    </div>
  )
}
