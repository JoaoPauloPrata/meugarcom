import { StrictMode, Suspense, lazy } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import App from './App.jsx'
import { AuthProvider } from './lib/auth.jsx'
import './styles.css'
import './app.css'

const Login = lazy(() => import('./pages/auth/Login.jsx'))
const Signup = lazy(() => import('./pages/auth/Signup.jsx'))
const ForgotPassword = lazy(() => import('./pages/auth/ForgotPassword.jsx'))
const ResetPassword = lazy(() => import('./pages/auth/ResetPassword.jsx'))
const DashboardLayout = lazy(() => import('./pages/dashboard/DashboardLayout.jsx'))
const Overview = lazy(() => import('./pages/dashboard/Overview.jsx'))
const MenuPage = lazy(() => import('./pages/dashboard/MenuPage.jsx'))
const TablesPage = lazy(() => import('./pages/dashboard/TablesPage.jsx'))
const ConversationsPage = lazy(() => import('./pages/dashboard/ConversationsPage.jsx'))
const SettingsPage = lazy(() => import('./pages/dashboard/SettingsPage.jsx'))
const ChatPage = lazy(() => import('./pages/chat/ChatPage.jsx'))

function PageFallback() {
  return (
    <div className="page-loading" role="status">
      <span className="spinner" aria-hidden="true" />
      <span className="sr-only">Carregando…</span>
    </div>
  )
}

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <Suspense fallback={<PageFallback />}>
          <Routes>
            <Route path="/" element={<App />} />
            <Route path="/login" element={<Login />} />
            <Route path="/signup" element={<Signup />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password" element={<ResetPassword />} />
            <Route path="/dashboard" element={<DashboardLayout />}>
              <Route index element={<Overview />} />
              <Route path="menu" element={<MenuPage />} />
              <Route path="tables" element={<TablesPage />} />
              <Route path="conversations" element={<ConversationsPage />} />
              <Route path="settings" element={<SettingsPage />} />
            </Route>
            <Route path="/m/:code" element={<ChatPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Suspense>
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
)
